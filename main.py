def main(args):
    for i in range(len(args)):
        if i == args[i]:
            return True
    return False

# Não alterar
if __name__ == "__main__":
    import sys
    args = sys.argv[1:]
    mainInput = []
    for arg in args:
        if arg.isnumeric():
            mainInput.append(int(arg))
        else:
            mainInput.append(arg)
    print(main(mainInput))