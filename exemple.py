def main(args):
    n = len(args)
    
    if n < 50:
        for i in range(n):
            if i == args[i]:
                return True
        return False

    i = 0
    j = n

    for k in range(20):
        pivo = int((i + j) / 2)
        value = args[pivo] - pivo

        if value < 0:
            i = pivo
        elif value > 0:
            j = pivo
        else:
            return True

    return False

# @@ Não alterar
if __name__ == "__main__":
    import sys
    path = sys.argv[1]

    mainInput = []
    with open(path, "r") as file:
        for line in file.readlines():
            mainInput.extend([float(i) for i in line.split(" ") if i.strip()])
    
    import time

    start = time.time()
    print(main(mainInput))
    end = time.time()
    print(end - start)
# @@