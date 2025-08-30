const fs = require("fs");
const path = require("path");

if (!fs.existsSync("Datas")) {
    fs.mkdirSync("Datas");
}

if (!fs.existsSync("Datas/AuthorImage")) {
    fs.mkdirSync("Datas/AuthorImage");
}

if (!fs.existsSync("Datas/Thumbnail")) {
    fs.mkdirSync("Datas/Thumbnail");
}

if (!fs.existsSync("Datas/Items")) {
    fs.mkdirSync("Datas/Items");
}

if (!fs.existsSync("Translate")) {
    fs.mkdirSync("Translate");
}

const TRANSLATE_FILES = "../../../Translate Files";
const translateFiles = fs.readdirSync(TRANSLATE_FILES);
const translateFilesLength = translateFiles.length;
console.log("Translate Files: " + translateFilesLength);

for (let i = 0; i < translateFilesLength; i++) {
    const file = translateFiles[i];
    const filePath = path.join(TRANSLATE_FILES, file);
    const destPath = path.join("Translate", file);
    fs.copyFileSync(filePath, destPath);
    console.log("Copying: " + file);
}

if (!fs.existsSync("Datas/Fonts")) {
    fs.mkdirSync("Datas/Fonts");
}

const FONT_FILES = "../../../Font Files";
const fontFiles = fs.readdirSync(FONT_FILES);
const fontFilesLength = fontFiles.length;
console.log("Font Files: " + fontFilesLength);

for (let i = 0; i < fontFilesLength; i++) {
    const file = fontFiles[i];
    const filePath = path.join(FONT_FILES, file);
    const destPath = path.join("Datas/Fonts", file);
    fs.copyFileSync(filePath, destPath);
    console.log("Copying: " + file);
}

const LICENSE_FILES = [
    "../../../LICENSE.txt",
    "../../../THIRD_PARTY_LICENSES.txt"
];
const LICENSE_DESTINATION = [
    "LICENSE.txt",
    "THIRD_PARTY_LICENSES.txt"
];

for (let i = 0; i < LICENSE_FILES.length; i++) {
    const file = LICENSE_FILES[i];
    const dest = LICENSE_DESTINATION[i];
    fs.copyFileSync(file, dest);
    console.log("Copying: " + dest);
}

fs.rmSync("build.js");
console.log("Build Completed!");
