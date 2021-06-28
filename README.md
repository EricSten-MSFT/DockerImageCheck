# DockerImageCheck
Simple tool to check docker image integrity

# Overview
* find the docker images dir from /etc/docker/daemon.json
* find the image catalog in respositories.json
* for each image in the catalog, run ```docker save ``` *image* ```  ``` , ignoring the output, and checking the exit code
  * if exit code is non-zero, the image is corrupt
