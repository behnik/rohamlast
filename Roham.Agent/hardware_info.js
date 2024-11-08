const si = require('systeminformation');
const { getInstalledApps } = require('get-installed-apps');

exports.get = async () => {
    var osinfo = await si.osInfo();

    var cpu = await si.cpu();

    var graphics = await si.graphics();

    var networkinterfaces = await si.networkInterfaces();

    var mem = await si.mem();

    var osinfo = await si.osInfo();

    var disklayout = await si.diskLayout();

    var user_dns_domain = '';
    if (process.env.USERDNSDOMAIN !== undefined) user_dns_domain = process.env.USERDNSDOMAIN;
    var user_domain = process.env.USERDOMAIN;

    var printers = await si.printer();

    var user_name = '';

    var users = await si.users();
    if (users.length > 0) user_name = users[0].user;

    var apps = await getInstalledApps();
    //console.log(apps);
    var installed_apps = [];
    apps.forEach((app) => {
        installed_apps.push({
            appIdentifier : app.appIdentifier,
            version : app.Version
        });
    });
    var info = {
        cpu,
        mem,
        osinfo,
        disklayout,
        graphics,
        printers,
        networkinterfaces,
        userinfo: {
            user_name,
            user_dns_domain,
            user_domain
        },
        apps: installed_apps
    };

    return info;
};