const { createCipheriv,createDecipheriv,scryptSync, randomBytes } = require("crypto");
const { Buffer } = require('node:buffer');

exports.salt = () => {
  return randomBytes(16).toString("hex");
};

exports.hash = (password,salt) => {
  return scryptSync(password, salt, 32).toString("hex");
};

exports.compare = (password, password_hashed, salt) => {
  // Any random string here (ideally should be atleast 16 bytes)

  //const salt = randomBytes(16).toString("hex");

  // Pass the password string and get hashed password back
  // ( and store only the hashed string along with the salt in your database)
  // {hashedPassword}${salt}

  var gen_hash = scryptSync(password, salt, 32).toString("hex");
    console.log(`${gen_hash}---${password_hashed}`);
  if (gen_hash === password_hashed) return true;
  else return false;
};

const password = "shared_key"
const algorithm = "aes256"
const iv = Buffer.alloc(16, 0); // Initialization vector.
exports.encrypt = (text) => {
    if (!text) return ''
    const cipher = createCipheriv(algorithm, password,iv);
    let crypted = cipher.update(text, 'utf-8', 'base64');
    crypted += cipher.final('base64');
    return crypted;
}

exports.decrypt = (text) => {
    if (!text) return ''
    const decipher = createDecipheriv(algorithm, password,iv);
    let decrypted = decipher.update(text, 'base64', 'utf-8');
    decrypted += decipher.final('utf-8');
    return decrypted;
}