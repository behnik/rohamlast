const db = require('./db');
const crypto_service = require('./cryptos');
const roles_service = require('./roles');
const jwt = require("jsonwebtoken");
const AD = require("@davistran86/ad");
const lock_service = require('./lock');

//#region public methods
exports.create_user = async (register_user) => {
    try {
        var user = await this.get_by_user_name(register_user.user_name);
        if (user.length === 0) {
            const salt = crypto_service.salt();
            var hashed_password = crypto_service.hash(register_user.password, salt);

            var result = await this.insert_user({
                user_name: register_user.user_name,
                hashed_password: hashed_password,
                first_name: register_user.first_name,
                last_name: register_user.last_name,
                tel: register_user.tel,
                mobile: register_user.mobile,
                address: register_user.address,
                national_code: register_user.national_code,
                sex: register_user.sex,
                role_id: register_user.role_id,
                salt: salt,
                unit_id: register_user.unit_id
            });

            var user_id = parseInt(result.insertId);
            if (user_id > 0)
                return {
                    code: 200,
                    message: "کاربر جدید با موفقیت ثبت شد.",
                };
            else return { code: 504, message: "خطا در ثبت کاربر جدید!" };
        }
        else return {
            code: 409,
            message: "امکان ثبت این نام کاربری وجود ندارد!",
        };
    } catch (e) {
        console.dir(e);
        return { code: 500, message: "خطا در ثبت کاربر جدید!", error: e };
    }
};

exports.change_password = async (user_name, current_password, new_password) => {
    try {
        var user = await this.get_by_user_name(
            user_name,
        );

        if (user !== null) {
            var match = crypto_service.compare(
                current_password,
                user[0].hashed_password,
                user[0].salt,
            );

            if (match) {
                var hashed_password = crypto_service.hash(new_password, user[0].salt);

                await this.update_password(user[0].id, hashed_password);

                return {
                    code: 200,
                    message: "رمز عبور با موفقیت تغییر یافت.",
                };
            } else
                return {
                    code: 503,
                    message: "رمز عبور فعلی صحیح نیست!",
                };
        }
        else {
            return {
                code: 404,
                message: "کاربری با این مشخصات یافت نشد!"
            };
        }

    } catch (e) {

    }
};

exports.change_user_password = async (user, new_password) => {
    try {
        if (user !== null) {
            var hashed_password = crypto_service.hash(new_password, user[0].salt);

            await this.update_password(user[0].id, hashed_password);

            return {
                code: 200,
                message: "رمز عبور با موفقیت تغییر یافت.",
            };
        }
        else {
            return {
                code: 404,
                message: "کاربری با این مشخصات یافت نشد!"
            };
        }

    } catch (e) {
        return {
            code: 500,
            message: "خطا در تغییر رمز عبور"
        };
    }
}

exports.authenticate = async (user_name, password) => {
    try {
        var login_type = await db.get_config("login_type");

        var current_user = undefined;

        var user = await this.get_by_user_name(user_name);

        if (user.length === 1 && user[0].is_active) {
            if (login_type === "0") {
                var match = crypto_service.compare(
                    password,
                    user[0].hashed_password,
                    user[0].salt,
                );
                if (match) current_user = user[0];
            }
            // ldap login
            else if (login_type === "1") {
                var ldap_username = await db.get_config("ldap_user_name"); //process.env.LDAP_USER_NAME;
                var ldap_password = await db.get_config("ldap_password"); //process.env.LDAP_PASSWORD;
                var ldap_url = await db.get_config("ldap_url"); //process.env.LDAP_URL;

                const ad = new AD({
                    url: ldap_url,
                    user: ldap_username,
                    pass: ldap_password,
                });

                var auth_result = await ad.user(user_name).authenticate(password);
                //console.log(auth_result);

                if (auth_result === true) current_user = user[0];
            }
        }

        if (current_user !== undefined) {
            var permissions_response =
                await roles_service.get_role_queries_by_role_id(current_user.role_id);

            var token = jwt.sign(
                {
                    id: current_user.id,
                    user_name: current_user.user_name,
                    first_name: current_user.first_name,
                    last_name: current_user.last_name,
                    permissions: permissions_response,
                    role_title: current_user.role_title,
                    role_id: current_user.role_id
                },
                process.env.TOKEN_SECRET,
                { expiresIn: process.env.TOKEN_EXPIRATIONTIME },
            );

            var permissions = [];

            permissions_response.forEach((p) => {
                permissions.push(p.url);
            });

            return {
                code: 200,
                data: {
                    user_id: current_user.id,
                    user_name: current_user.user_name,
                    role_name: current_user.role_title,
                    token: token,
                    permissions: permissions,
                    expires_in: 86400
                }
            };
        } else {
            return { code: 404, message: "کاربری با این مشخصات یافت نشد!" };
        }
    } catch (e) {
        console.dir(e);
        return { code: 500, error: e };
    }
};

exports.create_ldap_user = async (ldap_user) => {
    return await db.exec_query(`set @group_id = (select id from roles where is_default = 1);
                                            INSERT INTO 
                                            users (role_id,ldap_display_name, user_name, ldap_principal_name, ldap_groups,is_ldap_user) 
                                            VALUES (@group_id,:display_name, :account_name, :principal_name, :groups,1)
                                            ON DUPLICATE
                                            KEY UPDATE  ldap_display_name = :display_name,
                                                                    user_name = :account_name,
                                                                    ldap_groups=:groups,
                                                                    ldap_principal_name = :principal_name;
                                            `, ldap_user);
};

// authentication middleware
exports.check_authenticate = async (req, res, next) => {
    var authHeader = req.headers['authorization']
    var token = authHeader && authHeader.split(' ')[1];
    if (token !== null && token !== undefined) {
        jwt.verify(token, process.env.TOKEN_SECRET, (err, user) => {
            //if (err) return res.send({
            //    code: 401,
            //    message: 'ابتدا باید وارد سیستم شوید!',
            //    error: err
            //})

            if (err) req.user = null;

            else req.user = user

            next();
        })
    }
    else {
        req.user = null;
        next();
    }
};

//#endregion


//#region private methods
exports.get_by_user_name = async (user_name) => {
    return await db.exec_query(
        `SELECT u.id,u.user_name,u.salt,u.hashed_password,
                u.is_active,u.role_id,r.title as role_title
        FROM users u
        INNER JOIN roles r ON r.id = u.role_id
        WHERE user_name = :user_name`,
        { user_name: user_name }
    );
};

exports.get_by_user_id = async (user_id) => {
    return await db.exec_query(
        `SELECT u.id,u.user_name,u.salt,u.hashed_password,
                u.is_active,u.role_id,r.title as role_title
        FROM users u
        INNER JOIN roles r ON r.id = u.role_id
        WHERE u.id = :user_id`,
        { user_id: user_id }
    );
};

exports.update_password = async (user_id, hashed_password) => {
    return await db.exec_query(
        `update users 
        set hashed_password = :hashed_password
        where id = :user_id;`,
        {
            hashed_password: hashed_password,
            user_id: user_id
        });
};

exports.insert_user = async (user) => {
    return await db.exec_query(
        `insert into users(national_code,user_name,hashed_password,
            first_name,last_name,sex,tel,mobile,address,role_id,salt,unit_id)
        values(:national_code,:user_name,:hashed_password,:first_name,
            :last_name,:sex,:tel,:mobile,:address,:role_id,:salt,:unit_id)`,
        user
    );
};
//#endregion


//#region routes

exports.init_routes = async(app)=>{
    app.post('/api/users/login',
        [lock_service.check],
        async (req, res) => {
            var user_name = req.body.user_name;
            var password = req.body.password;

            res.send(await this.authenticate(user_name, password));
        });

    app.post('/api/users/change-password',
        [lock_service.check, this.check_authenticate],
        async (req, res) => {
            var current_user = req.user;
            var current_password = req.body.current_password;
            var new_password = req.body.new_password;

            res.send(await this.change_password(current_user.user_name, current_password, new_password));
        });

    app.post('/api/users/user-change-password',
        [lock_service.check, this.check_authenticate],
        async (req, res) => {
            var user_id = req.body.user_id;
            var new_password = req.body.new_password;

            var user = await this.get_by_user_id(user_id);

            res.send(await this.change_user_password(
                user,
                new_password)
            );
        });
};

//#endregion