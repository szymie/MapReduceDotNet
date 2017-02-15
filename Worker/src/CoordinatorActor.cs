using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using System.Collections;

namespace Worker
{
	public abstract class CoordinatorActor : TypedActor, IHandle<NewWorkMessage>, IHandle<RegisterCoordinatorAckMessage>, IHandle<WorkerFailureMessage>, IHandle<AbortWorkMessage>, IHandle<MonitorSystemInfo>
	{
		protected int cpuProcessingQueueLength;

		protected Dictionary<IActorRef, WorkerConfig> workers = new Dictionary<IActorRef, WorkerConfig>();
		private UniqueKeyGenerator keyGenerator = new UniqueKeyGenerator();
		protected int CoordinatorId{get;set;} = -1;
		protected ActorSelection MasterActor{ get; set; }
		private PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
		private Queue<float> cpuLoadQueue = new Queue<float> ();


		public CoordinatorActor ()
		{
			MasterActor = getMasterActorRef ();
			this.cpuProcessingQueueLength = getCpuProcessingQueueLength();
		}

		public void Handle(MonitorSystemInfo m){
			cpuLoadQueue.Enqueue (cpuCounter.NextValue ());
			if (cpuLoadQueue.Count > 10) {
				cpuLoadQueue.Dequeue ();
			}
			float result = 0;
			cpuLoadQueue.ToList ().ForEach (value => result = result + value);
			result = result / cpuLoadQueue.Count;
			Console.WriteLine (String.Format("Cpu usage from last {0} measurements: {1}%", cpuLoadQueue.Count, result));
			MasterActor.Tell(new CoordinatorSystemInfo(result, CoordinatorId));
		}


		ActorSelection getMasterActorRef ()
		{
			string masterAddress = Environment.GetEnvironmentVariable ("MASTER_ADDRESS");
			if (masterAddress == null) {
				masterAddress = "localhost:8081";
				Console.WriteLine ("No MASTER_ADDRESS found.");
			}

			Console.WriteLine ("MASTER_ADDRESS {0}", masterAddress);

			return Context.ActorSelection("akka.tcp://MasterSystem@" + masterAddress + "/user/MasterActor");
		}

		private int getCpuProcessingQueueLength(){
			int cpuProcessingQueueLengthInt = 10;
			string cpuProcessingQueueLengthString = Environment.GetEnvironmentVariable ("CPU_QUEUE_LENGTH");
			if (cpuProcessingQueueLengthString == null) {				
				Console.WriteLine ("No CPU_QUEUE_LENGTH found.");
			} else {
				cpuProcessingQueueLengthInt = Int32.Parse(cpuProcessingQueueLengthString);
			}

			Console.WriteLine ("CPU_QUEUE_LENGTH {0}", cpuProcessingQueueLengthInt.ToString());

			return cpuProcessingQueueLengthInt;
		}

		public void Handle (NewWorkMessage message)
		{
			Console.WriteLine ("New work message: " + CoordinatorId);
			int workerId = keyGenerator.generateKey();

			MasterActor.Tell (new NewWorkAckMessage(workerId, message.OrderedWorkId, message.WorkConfig.TaskId, CoordinatorId));

			WorkerConfig workerConfig = new WorkerConfig (workerId, message.WorkConfig, CoordinatorId);

			startNewWorker (workerConfig);
		}

		private void startNewWorker(WorkerConfig workerConfig){
			IActorRef workerActor = createWorkerActor(workerConfig.WorkerId);

			workers.Add (workerActor, workerConfig);
			Context.Watch (workerActor);
			workerActor.Tell(new NewWorkerMessage(workerConfig));
		}

		public void Handle (RegisterCoordinatorAckMessage message)
		{
			Console.WriteLine ("Registered with id: " + message.CoordinatorId);
			this.CoordinatorId = message.CoordinatorId;
			Context.System.Scheduler.ScheduleTellRepeatedly (new TimeSpan(0, 0, 1), new TimeSpan(0,0,1), Self, new MonitorSystemInfo(),Self);
		}

		public void Handle (WorkerFailureMessage message){
			Console.WriteLine ("Worker failed: " + message.TaskId + "-" + CoordinatorId + "-" + message.WorkerId);
			Console.WriteLine (message.Message);
			MasterActor.Tell (message);

			workers.Remove (Sender);
			Context.Stop (Sender);
		}

		public void Handle(AbortWorkMessage message){
			Console.WriteLine ("Aborting worker: " + CoordinatorId + "-" + message.WorkerId);		

			foreach(IActorRef worker in workers.Keys.ToList()){
				WorkerConfig workerConfig = workers [worker];
				if(workerConfig.WorkerId == message.WorkerId){
					workers.Remove (worker);

					worker.Tell (new StopWorkerMessage());

					return;
				}
			}
		}

		public void Handle (Terminated message)
		{			
			WorkerConfig workerConfig;
			if (workers.TryGetValue (message.ActorRef, out workerConfig)) {
				LocalFilesDirectory dir = new LocalFilesDirectory (workerConfig);
				dir.removeDirectory ();

				Console.WriteLine ("Restarting worker...");

				startNewWorker (workerConfig);
			}
		}

		protected abstract IActorRef createWorkerActor(int workerId);
	}
}