def main(args):
    return "não implementado"

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