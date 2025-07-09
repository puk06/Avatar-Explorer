const fs = require('fs');

const files = [
    { path: "Forms/MainForm.Designer.cs", replacement: "GuiFont" },
    { path: "Forms/AddItemForm.Designer.cs", replacement: "_mainForm.GuiFont" },
    { path: "Forms/SelectSupportedAvatarForm.Designer.cs", replacement: "_mainForm.GuiFont" },
    { path: "Forms/ManageCommonAvatarsFrom.Designer.cs", replacement: "_mainForm.GuiFont" },
    { path: "Forms/SelectAutoBackupForm.Designer.cs", replacement: "_mainForm.GuiFont" },
    { path: "Forms/AddNoteForm.Designer.cs", replacement: "_mainForm.GuiFont" }
];

files.forEach(({ path, replacement }) => {
    try {
        const data = fs.readFileSync(path, "utf-8");
        const replacedData = data.replace(new RegExp(`"${target}"`, "g"), `"Noto Sans JP"`);
        fs.writeFileSync(path, replacedData);
        console.log(`Replaced in ${path}`);
    } catch (err) {
        console.error(`Error processing ${path}:`, err.message);
    }
});