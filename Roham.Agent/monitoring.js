const checkDiskSpace = require('check-disk-space').default
const os = require('os');
const si = require('systeminformation');

exports.init = async () => {

    const { cpuUsage } = require('process');
    const startUsage = cpuUsage();

    // spin the CPU for 500 milliseconds
    const now = Date.now();
    while (Date.now() - now < 500);

    var cpu_usage = cpuUsage(startUsage).user;
    var cpu_used = (((cpu_usage / 1024) / 1024) * 100).toPrecision(2);

    var free_mem = (((os.freemem() / 1024) / 1024) / 1024).toPrecision(2);
    var total_mem = (((os.totalmem() / 1024) / 1024) / 1024).toPrecision(2);

    var host_name = os.hostname();
    var diskSpace = await checkDiskSpace('C:/');

    var free_disk = (((diskSpace.free / 1024) / 1024) / 1024);
    var total_disk = (((diskSpace.size / 1024) / 1024) / 1024);

    var ns = await si.networkStats();

    return {
        computer_name: host_name,
        info: {
            cpu: {
                used: cpu_used
            },
            ram: {
                free: free_mem,
                total: total_mem
            },
            disk: {
                free: free_disk,
                total: total_disk
            },
            network: {
                receive: ns[0].rx_sec,
                send: ns[0].tx_sec
            }
        }
    };

};