#!/bin/bash
sleep 10
systemctl enable entry-point.service
systemctl start entry-point.service
