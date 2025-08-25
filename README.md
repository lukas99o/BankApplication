<p align="center">
  <img width="320" alt="Retro Bank 3000 Logo" src="https://github.com/user-attachments/assets/f2fad855-9c00-443e-950b-c38456930d65" />
</p>

# 🎮 Introduction
Retro Bank 3000 is a .NET Console Bank Application with an SQLite database.  
I created this project to improve on the original version that me and my team developed during my studies at **Chas Academy**.  

---

## 🚀 Background  
The first project we built helped us to:  
- Learn the basics of databases  
- Understand how to work with **Code-First Database Principles**  
- Develop features and **reusable code**  

Afterwards I felt a strong passion to improve the project, especially after some other students showed what was actually possible in a pure **CMD Console application**.  

---

## 🛠️ What I have learned in this project  
In Retro Bank 3000 I have:  
- Deepened my understanding of **code reusability** (a function is used over **50 times** in the project).  
- Worked with **SQLiteDb** to understand more database concepts.  
  - The choice of SQLite was perfect considering the size and simplicity of the database that was needed.  
- Explored what is possible with the **Console class in .NET**, among others:  
  - `Console.Beep()` → to produce sounds  
  - `Console.ForegroundColor` & `Console.BackgroundColor` → to color text and background  
- Built my own menu system that:  
  - Lets the user navigate with **arrow keys** and **Enter**  
  - Always centers text in the console (via `CenterY` and `CenterX` functions)  
- Learned how to **hash passwords securely** by implementing my own `PasswordHash` class that uses:  
  - **Salt** to prevent rainbow table attacks  
  - **Hashing (SHA-256/512)** to protect the passwords  

---

## 🎲 Features  
The project contains both serious and fun features:  

✅ **Bank functionality** – accounts, deposits, withdrawals, transactions  
✅ **Registration & Login** – with password management (**hashing + salt**)  
✅ **Admin menu** – special features for administrators  
✅ **Casino/Gambling feature** – chance to win or lose virtual money  
✅ **Leaderboard** – compare your results with other users  
✅ **Bonus Room** 🎉 – a part of the application with animations and effects  

---

## 📦 Installation & Setup  
1. Clone the repo to your computer  
2. Open the project in **Visual Studio**  
3. Use **NuGet Package Manager Console** and run:  

   ```powershell
   update-database
