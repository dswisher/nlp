
import argparse
import gzip


def main():
    parser = argparse.ArgumentParser("Peek at pages in a wikipedia archive file.")

    parser.add_argument('--file', default='../data/enwiki-20200120-pages-meta-current1.xml-p10p30303.gz')
    parser.add_argument('--page', default=1)

    args = parser.parse_args()

    with gzip.open(args.file, 'rt') as fp:
        desired_page = int(args.page)
        current_page = 0

        in_page = False

        while True:
            content = fp.readline().rstrip('\n')
            if in_page:
                if current_page == desired_page:
                    print(content)
                if content.strip() == '</page>':
                    in_page = False
            else:
                if content.strip() == '<page>':
                    in_page = True
                    current_page += 1
                    if current_page == desired_page:
                        print(content)
                    elif current_page > desired_page:
                        break


main()
