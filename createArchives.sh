#!/bin/bash
tar -cvzf /tmp/Master.tar.gz -C ./Master/bin/Debug .
tar -cvzf /tmp/Worker.tar.gz -C ./Worker/bin/Debug .
tar -cvzf /tmp/EntryPoint.tar.gz -C ./EntryPoint/bin/Debug .
