counterIDS1 = 0
totalIDS1 = 0
counterIDS3 = 0
totalIDS3 = 0
counterMinimax = 0
totalMinimax = 0

with open("file.txt", "r") as f:
    for line in f:
        if line.startswith("IDS_D5T1") and line.endswith("s\n"):
            counterIDS1 += 1
            timewiths = line.split(" ")[-1]
            time=float(timewiths.replace("s\n", "").replace(",","."))
            totalIDS1 += time
        if line.startswith("IDS_D5T3") and line.endswith("s\n"):
            counterIDS3 += 1
            timewiths = line.split(" ")[-1]
            time=float(timewiths.replace("s\n", "").replace(",","."))
            totalIDS3 += time
        if line.startswith("MinimaxD5") and line.endswith("s\n"):
            counterMinimax += 1
            timewiths = line.split(" ")[-1]
            time=float(timewiths.replace("s\n", "").replace(",","."))
            totalMinimax += time

print("Counter IDS Euristica Default: ", counterIDS3)
print("Media IDS Euristica Default:", totalIDS3/counterIDS3)

print("Counter IDS Nostra Euristica : ", counterIDS1)
print("Media IDS Nostra Euristica:", totalIDS1/counterIDS1)

print("Counter Minimax: ", counterMinimax)
print("Media Minimax:", totalMinimax/counterMinimax)
