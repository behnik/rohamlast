const pages_service = require('./pages');
const moment = require('moment');
const db = require('./db');
const crypto_service = require('./cryptos');

exports.check = async (req, res, next) => {

    //const environment = process.env.NODE_ENV || 'production';
    //console.log(environment);
    //var lock_check = req.app.get('lock_check');
    //console.log(lock_check);

    //if (environment !== 'development') {
    //    if (lock_check.exist === false) {
    //        var page_response = await pages_service.get_by_page_url('admin', 'locknotfound');
    //        res.send(page_response[0].page_content);
    //    }
    //    else {
    //        var checked_at = lock_check.checked_at;
    //        console.log(checked_at);
    //        var diff = moment(checked_at).fromNow();
    //        console.log(diff);

    //        if (diff === 'a few seconds ago' ||
    //            diff === 'a minute ago') {
    //            next();
    //        }
    //        else {
    //            var page_response = await pages_service.get_by_page_url('admin', 'locknotfound');
    //            res.send(page_response[0].page_content);
    //        }
    //    }
    //}
    //else next();

    //var system_config = await db.exec_single_query('select id,name,value,updated_at from configs where name=:name;', {
    //    name: 'system_check'
    //});

    //try {
    //    var decrypted = crypto_service.decrypt(system_config.value);

    //    console.log(decrypted);

    //    if (decrypted !== null && decrypted !== '') {
    //        var lock_status = JSON.parse(decrypted);
    //        console.log(lock_status);
    //    }
    //} catch (e) {

    //}
    next();
};

exports.set = async (lock_status) => {
    //var encrypted = crypto_service.encrypt(JSON.stringify(lock_status));

    //await db.exec_query(`update configs set value=:value,updated_at=current_timestamp() where name=:name;`,
    //    {
    //        name: 'system_check',
    //        value: encrypted
    //    });
};