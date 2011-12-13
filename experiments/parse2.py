import sys

def get_vals_from_file(filename):
    f = open(filename, 'r')
    for line in f:
        if len(line) > 0 and line[0] != 'g':
            yield line

def append_lines(results, filename):
    gen = 0
    for line in get_vals_from_file(filename):
        results[gen].append(line)
        gen += 1

def write_results_to_file(avg, best, counts, filename):
	f = open(filename, 'w')
	f.write('Generation,Average,Best,Samples\n')
	for i in range(len(avg)):
		f.write('{0},{1},{2},{3}\n'.format(i, avg[i], best[i], counts[i]))

def main():
    lines = []
    name = sys.argv[1] + '_results'
    for i in range(501):
    	lines.append([])
    # Load each line into the arrays
    for i in range(30):
    	append_lines(lines, name + str(i) + '.csv')
    avg = []
    best = []
    counts = []
    # Process each generation
    for i in range(500):
    	counts.append(len(lines[i]))
    	if counts[i] > 0:
    		avg.append(sum(float(x.split(',')[1]) for x in lines[i]) / counts[i])
    		best.append(sum(float(x.split(',')[2]) for x in lines[i]) / counts[i])
    write_results_to_file(avg, best, counts, 'average_' + name + '.csv')


if __name__ == "__main__":
    main()