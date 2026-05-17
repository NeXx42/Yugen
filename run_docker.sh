docker stop yugen
docker rm yugen

docker build -t yugen .
docker run --name yugen -p 3000:3000 yugen