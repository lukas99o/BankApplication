<p align="center">
  <img width="320" alt="Retro Bank 3000 Logo" src="https://github.com/user-attachments/assets/f2fad855-9c00-443e-950b-c38456930d65" />
</p>

# 🎮 Introduktion
Retro Bank 3000 är en .NET Console Bank Application med SQLite-databas.  
Jag skapade detta projekt för att förbättra på den ursprungliga versionen som jag och mitt team utvecklade under mina studier på **Chas Academy**.  

---

## 🚀 Bakgrund  
Det första projektet vi byggde hjälpte oss att:  
- Lära oss grunderna i databaser  
- Förstå hur man arbetar med **Code-First Database Principles**  
- Utveckla funktioner och **återanvändbar kod**  



Efteråt kände jag en stark passion för att förbättra projektet, särskilt efter att några andra studenter visade vad som faktiskt var möjligt i en ren **CMD Console-applikation**.  

---

## 🛠️ Vad jag har lärt mig i detta projekt  
I Retro Bank 3000 har jag:  
- Fördjupat min förståelse för **kodåteranvändning** (en funktion används över **50 gånger** i projektet).  
- Arbetat med **SQLiteDb** för att förstå fler databaskoncept.  
  - Valet av SQLite passade perfekt med tanke på storleken och enkelheten i databasen som behövdes.  
- Utforskat vad som är möjligt med **Console-klassen i .NET**, bland annat:  
  - `Console.Beep()` → för att producera ljud  
  - `Console.ForegroundColor` & `Console.BackgroundColor` → för att färglägga text och bakgrund  
- Byggt ett eget menysystem som:  
  - Låter användaren navigera med **piltangenter** och **Enter**  
  - Alltid centrerar text i konsolen (via `CenterY` och `CenterX` funktioner)  
- Lärt mig att **hasha lösenord på ett säkert sätt** genom att implementera en egen `PasswordHash`-klass som använder:  
  - **Salt** för att förhindra rainbow table-attacker  
  - **Hashing (SHA-256/512)** för att skydda lösenorden  

---

## 🎲 Features  
Projektet innehåller både seriösa och lite roliga funktioner:  

✅ **Bankfunktionalitet** – konton, insättningar, uttag, transaktioner  
✅ **Registrering & Inloggning** – med lösenordshantering (**hashing + salt**)  
✅ **Admin-meny** – särskilda funktioner för administratörer  
✅ **Casino/Gambling-funktion** – chans att vinna eller förlora virtuella pengar  
✅ **Leaderboard** – jämför dina resultat med andra användare  
✅ **Bonus Room** 🎉 – en del av applikationen med animationer och effekter  

---

## 📦 Installation & Setup  
1. Klona repot till din dator  
2. Öppna projektet i **Visual Studio**  
3. Använd **NuGet Package Manager Console** och kör:  

   ```powershell
   update-database
   ```
