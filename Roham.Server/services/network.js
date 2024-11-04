const dns = require('node:dns');
const os = require('node:os');

const options = { family: 4 };

exports.get_current_ip = (call_back) => {
    dns.lookup(os.hostname(), options, (err, addr) => {
        if (err) {
            console.error(err);
            throw new Error('IPv4 not found.');
        } else {
            console.log(`IPv4 address: ${addr}`);
            if (call_back) call_back(addr);
        }
    });    
};