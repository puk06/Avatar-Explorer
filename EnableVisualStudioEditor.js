const fs = require('fs');
const fontName = "Noto Sans JP";

const data = fs.readFileSync("Forms/Main.Designer.cs", "utf-8");
const replacedData = data.replace(/GuiFont/g, `"${fontName}"`);
fs.writeFileSync("Forms/Main.Designer.cs", replacedData);

const data2 = fs.readFileSync("Forms/AddItem.Designer.cs", "utf-8");
const replacedData2 = data2.replace(/_mainForm.GuiFont/g, `"${fontName}"`);
fs.writeFileSync("Forms/AddItem.Designer.cs", replacedData2);

const data3 = fs.readFileSync("Forms/SelectSupportedAvatar.Designer.cs", "utf-8");
const replacedData3 = data3.replace(/_mainForm.GuiFont/g, `"${fontName}"`);
fs.writeFileSync("Forms/SelectSupportedAvatar.Designer.cs", replacedData3);

const data4 = fs.readFileSync("Forms/ManageCommonAvatars.Designer.cs", "utf-8");
const replacedData4 = data4.replace(/_mainForm.GuiFont/g, `"${fontName}"`);
fs.writeFileSync("Forms/ManageCommonAvatars.Designer.cs", replacedData4);