#!/bin/bash
runuser -l ubuntu -c 'python3 /home/ubuntu/boto.py'
systemctl enable entry-point.service
systemctl start entry-point.service
