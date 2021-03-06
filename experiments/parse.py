import sys

def get_vals_from_file(filename):
    f = open(filename, 'r')
    line_num = -1
    for line in f:
        line_num += 1
        if len(line) > 0 and line_num > 0:
            yield line

def append_lines(results, filename):
    gen = 0
    for line in get_vals_from_file(filename):
        results[gen].append(line)
        gen += 1

def write_results_to_file(avg, best, updates, counts, filename):
    f = open(filename, 'w')
    f.write('Generation,Average,Best,Updates,Samples\n')
    for i in range(len(avg)):
        if len(updates) > 0:
            f.write('{0},{1},{2},{3},{4}\n'.format(i, avg[i], best[i], updates[i], counts[i]))
        else:
            f.write('{0},{1},{2},{3}\n'.format(i, avg[i], best[i], counts[i]))

def main():
    if(len(sys.argv) != 4):
        print 'Usage: python parse.py <prefix> <inclusive start index> <exclusive end index>'
        return
    lines = []
    name = sys.argv[1] + '_results'
    for i in range(501):
    	lines.append([])
    start_offset = int(sys.argv[2])
    end_offset = int(sys.argv[3])
    # Load each line into the arrays
    for i in range(start_offset, end_offset):
    	append_lines(lines, name + str(i) + '.csv')
    avg = []
    best = []
    updates = []
    counts = []
    # Process each generation
    for i in range(500):
    	counts.append(len(lines[i]))
    	if counts[i] > 0:
            avg.append(sum(float(x.split(',')[1]) for x in lines[i]) / counts[i])
            best.append(sum(float(x.split(',')[2]) for x in lines[i]) / counts[i])
            #updates.append(sum(int(x.split(',')[3]) for x in lines[i]) / counts[i])
    write_results_to_file(avg, best, updates, counts, 'average_' + name + '.csv')


if __name__ == "__main__":
    main()