#!/bin/bash
runuser -l ubuntu -c 'python3 /home/ubuntu/boto.py'
systemctl enable master.service
systemctl start master.service
