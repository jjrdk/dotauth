docker-machine regenerate-certs dev

SET DOCKER_TLS_VERIFY=1
SET DOCKER_HOST=tcp://192.168.56.102:2376
SET DOCKER_CERT_PATH=C:\Users\habar\.docker\machine\machines\dev
SET DOCKER_MACHINE_NAME=dev

docker-machine start dev
docker build -t simpleidserver .
docker run -t -d -p 80:5000 simpleidserver