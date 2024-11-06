const pages_service = require('./pages');

exports.check = async (req, res, next) => {
    try {
        var lock_status = JSON.parse(process.env.LOCK_STATUS);
        console.log(lock_status);
        next();
    } catch (e) {
        var page_response = await pages_service.get_by_page_url('admin', 'locknotfound');
        res.send(page_response[0].page_content);
    }
};