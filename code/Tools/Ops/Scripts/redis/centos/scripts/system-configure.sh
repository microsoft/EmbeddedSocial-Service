#!/bin/sh
#
# Usage: system-configure.sh -pp <port> -vp <port> -n <redisname>
#

while [ "$1" != "" ]; do
    case $1 in
        -pp | --persist_port ) shift
                        pprt=$1
                        ;;
        -vp | --volatile_port ) shift
                        vprt=$1
                        ;;
	-n | --name )   shift
                        redisname=$1
                        ;;
    esac
    shift
done

# check for necessary input files
if [ ! -r "thpdisable.service" ]; then
    echo "File thpdisable.service is required but cannot be found."
    echo "Exiting..."
    exit 1;
fi

if [ ! -r "redis-persistent-${redisname}.service" ]; then
    echo "File redis-persistent-${redisname}.service is required but cannot be found."
    echo "Exiting..."
    exit 1;
fi

if [ ! -r "redis-volatile-${redisname}.service" ]; then
    echo "File redis-volatile-${redisname}.service is required but cannot be found."
    echo "Exiting..."
    exit 1;
fi

if [ ! -r "redis-persistent-${redisname}.conf" ]; then
    echo "File redis-persistent-${redisname}.conf is required but cannot be found."
    echo "Exiting..."
    exit 1;
fi

if [ ! -r "redis-volatile-${redisname}.conf" ]; then
    echo "File redis-volatile-${redisname}.conf is required but cannot be found."
    echo "Exiting..."
    exit 1;
fi


echo ============================
echo Disable Transparent Huge Pages
echo ============================

sudo cp thpdisable.service /usr/lib/systemd/system/
sudo systemctl enable thpdisable
sudo systemctl start thpdisable

echo ============================
echo Turning on firewall daemon
echo ============================
sudo systemctl enable firewalld
sudo systemctl start firewalld

echo Opening ports $pprt and $vprt
sudo firewall-cmd --zone=public --add-port=${pprt}/tcp --permanent
sudo firewall-cmd --zone=public --add-port=${vprt}/tcp --permanent
sudo firewall-cmd --reload

echo ============================
echo Configure SElinux to enable ports for redis
echo ============================
sudo semanage port -a -t redis_port_t -p tcp ${pprt}
sudo semanage port -a -t redis_port_t -p tcp ${vprt}

echo ============================
echo OS settings to prevent redis warnings
echo ============================
echo "vm.overcommit_memory = 1" | sudo tee --append /etc/sysctl.d/99-redis.conf
echo "net.core.somaxconn = 4096" | sudo tee --append /etc/sysctl.d/99-redis.conf
sudo sysctl --system

echo ============================
echo copy and enable redis configurations
echo ============================

sudo cp "redis-persistent-${redisname}.conf" /etc/
sudo cp "redis-volatile-${redisname}.conf" /etc/

sudo cp "redis-persistent-${redisname}.service" /usr/lib/systemd/system/
sudo cp "redis-volatile-${redisname}.service" /usr/lib/systemd/system/
sudo systemctl disable redis

sudo systemctl enable "redis-persistent-${redisname}.service"
sudo systemctl start "redis-persistent-${redisname}.service"
sudo systemctl enable "redis-volatile-${redisname}.service"
sudo systemctl start "redis-volatile-${redisname}.service"
