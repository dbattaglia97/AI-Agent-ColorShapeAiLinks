counterIDS1 = 0
totalIDS1 = 0
counterIDS3 = 0
totalIDS3 = 0
counterMinimax = 0
totalMinimax = 0
nRipetizioni=-1
draw=False
def check_number(x):
    for n in range(1, x + 1):
        if n * (n - 1) == x:
            return n
    return -1

with open("file.txt", "r") as f:
    for line in f:
        if line.startswith("Game") and line.endswith("won\n"):
            words = line.split(" ")
            index= words.index("Over,")
            winner= words[index+1]
            if winner == "IDS_D10000T1":
                counterIDS1 += 1
            elif winner == "IDS_D10000T3":
                counterIDS3+=1
            else :
                draw=True


nRipetizioni=check_number(counterIDS1+counterIDS3)
if(nRipetizioni != -1):
    if not draw:
        print("Non ci sono pareggi")
    print("Vittorie IDS Euristica Default con partite con se stesso: ", counterIDS3)
    print("Vittorie IDS Nostra Euristica con partite con se stesso: ", counterIDS1)
    print("Vittorie IDS Euristica Default: ", counterIDS3- ((nRipetizioni/2)*((nRipetizioni/2)-1)))
    print("Vittorie IDS Nostra Euristica : ", counterIDS1- ((nRipetizioni/2)*((nRipetizioni/2)-1)))
else:
    if draw:
        print("Ci sono pareggi")
    print("Ci son stati pareggi, verifica tu")
    print("Vittorie IDS Euristica Default: ", counterIDS3)
    print("Vittorie IDS Nostra Euristica : ", counterIDS1)
