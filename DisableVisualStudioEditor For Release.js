const fs = require('fs');

const data = fs.readFileSync("Forms/Main.Designer.cs", "utf-8");
const replacedData = data.replace(/"Noto Sans JP"/g, "GuiFont");
fs.writeFileSync("Forms/Main.Designer.cs", replacedData);

const data2 = fs.readFileSync("Forms/AddItem.Designer.cs", "utf-8");
const replacedData2 = data2.replace(/"Noto Sans JP"/g, "_mainForm.GuiFont");
fs.writeFileSync("Forms/AddItem.Designer.cs", replacedData2);

const data3 = fs.readFileSync("Forms/SelectSupportedAvatar.Designer.cs", "utf-8");
const replacedData3 = data3.replace(/"Noto Sans JP"/g, "_mainForm.GuiFont");
fs.writeFileSync("Forms/SelectSupportedAvatar.Designer.cs", replacedData3);

const data4 = fs.readFileSync("Forms/ManageCommonAvatars.Designer.cs", "utf-8");
const replacedData4 = data4.replace(/"Noto Sans JP"/g, "_mainForm.GuiFont");
fs.writeFileSync("Forms/ManageCommonAvatars.Designer.cs", replacedData4);
