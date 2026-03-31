# Jellyfin 2 Samsung (Orsay)

<p align="center">
  <img src="https://github.com/PatrickSt1991/Samsung-Jellyfin-Installer/blob/master/.github/jellyfin-tizen-logo.svg" width="250" height="250" />
</p>

<div align="center">
  <p>A simple cross-platform installer to package and install the <strong>Jellyfin Orsay widget</strong> on Samsung Smart TVs (2012–2015).</p>

  <a href="https://discord.gg/7mga3zh8Cv"><img src="https://img.shields.io/badge/Ask%20it%20on%20Discord-7289DA?style=for-the-badge&logo=discord&logoColor=white" /></a>

  ![OS Support](https://img.shields.io/badge/Windows-Alpha-yellow?style=for-the-badge)
  ![OS Support](https://img.shields.io/badge/Linux-Alpha-yellow?style=for-the-badge)
  ![OS Support](https://img.shields.io/badge/macOS-Alpha-yellow?style=for-the-badge)
  
  <img src="https://img.shields.io/badge/🌐_Available_in-_Dutch,_English-blue?style=for-the-badge" />
  <br/>
  🇳🇱 🇬🇧
</div>

<img width="582" height="1012" alt="image" src="https://github.com/user-attachments/assets/d12161cb-dfe6-41b6-ba96-034ad2146d4f" />

---

## ✨ What it does

- Packages the Jellyfin Orsay widget
- Runs a local web server for TV installation
- Automatically generates `widgetlist.xml`
- Works on **Windows, macOS, and Linux**
- Multi-language UI

---

## 📺 How to use

1. Enable **Developer Mode** on your Samsung TV (see below)
2. Start the installer
3. Note the **PC IP address** shown in the app
4. Click **Build & Start**
5. On the TV: follow the **series-specific sync path** below

That’s it.

---

## 🛠️ Enable Developer Mode (Samsung Orsay TVs)

To install community apps on Samsung **Orsay-based Smart TVs (2012–2015)**, Developer Mode must be enabled.

### 1️⃣ Log in as developer

1. Open **Smart Hub** on the TV  
2. Press the **RED** button on the remote to open the login screen  
3. Log in with:
   - **Username:** `develop`
   - **Password:** `000000`  
     *(On some models the password may be empty)*

---

### 2️⃣ Open the install menu

After logging in as `develop`, use the path that matches your TV series:

- **E series (2012):** press the **BLUE** button on the Smart Hub screen  
  **or**
  open **Tools → Settings** (model dependent) to open the **Developer Menu**
- **F series (2013):** open **Smart Hub → More Apps** (bottom right), then open **Options** (top right)
- **H / compatible 2015 Orsay series:** in **Smart Hub**, highlight any app and hold the center button until the sync menu appears

---

### 3️⃣ Set the Server IP

In the menu you opened above:

1. Select **Setting Server IP** on E series or **IP Setting** on F/H series
2. Enter the **IP address shown in the installer**
3. Confirm and save

This tells the TV where to fetch the widget from.

---

### 4️⃣ Sync User Applications

Still in the same menu:

1. Select **User Application Synchronization** on E series, **Start App Sync** on F series, or **Start User App Sync** on H/compatible 2015 Orsay series
2. Wait for the sync to complete

The Jellyfin app will now appear in Smart Hub.

---

### ℹ️ Notes

- These steps apply to **Orsay TVs only** (roughly 2012–2015)
- Menu names may vary slightly by firmware version
- Newer Samsung TVs using **Tizen** are **not supported**

Developer mode instructions are based on the Samsung Orsay community documentation:
https://emby.media/community/index.php?/topic/9869-samsung-orsay-smarttv-2011-2015-community-app-install-instructions/

---

## 🛡️ Antivirus warning

Some antivirus tools (including Windows Defender) may flag the installer as suspicious.  
This is a **false positive**, common for self-contained installers.

You can verify integrity using the provided **SHA256 checksum** in the release assets.

---

## ❤️ Support

If this saved you time, consider buying me a beer ☕  
Feedback and issues are welcome.

- https://github.com/PatrickSt1991/Samsung-Jellyfin-Installer
