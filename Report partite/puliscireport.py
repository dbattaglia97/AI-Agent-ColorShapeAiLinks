# Apri il file in modalità lettura
with open('file.txt', 'r') as file:
    # Leggi tutte le righe del file
    lines = file.readlines()

lines = [line for line in lines if (line.startswith("MinimaxD3") or line.startswith("MinimaxD5") or line.startswith("IDS_D3T1") or line.startswith("Random") or line.startswith("IDS_D3T3") or line.startswith("Game")or line.startswith("IDS_D5T3")or line.startswith("IDS_D5T1")or line.startswith("IDS_D1000T3")or line.startswith("IDS_D1000T1") or line.startswith("IDS_D10000T3")or line.startswith("IDS_D10000T1"))]
# Rimuovi le righe vuote
lines = [line for line in lines if line.strip() != '']

# Apri il file in modalità scrittura
with open('file.txt', 'w') as file:
    # Scrivi tutte le righe non vuote nel file
    file.writelines(lines)
