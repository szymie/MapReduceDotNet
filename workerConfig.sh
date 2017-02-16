#!/bin/bash
sleep 10
systemctl enable worker.service
systemctl start worker.service
