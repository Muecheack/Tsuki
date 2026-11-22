wt `
   new-tab --title "python端" -d "D:\AllMyProjects\Tsuki\pyai" pwsh -NoExit -Command "python app.py" `
  `; new-tab --title ".Net端" -d "D:\AllMyProjects\Tsuki\cs\cs\cs" pwsh -NoExit -Command "dotnet run"

Start-Sleep -Seconds 7
wt --window 0 new-tab --title "前端" -d "D:\AllMyProjects\Tsuki\Face\bot-face" pwsh -NoExit -Command "npm run dev"
