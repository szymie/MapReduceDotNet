#!/bin/bash
runuser -l ubuntu -c 'python3 /home/ubuntu/boto.py'
systemctl enable worker.service
systemctl start worker.service
