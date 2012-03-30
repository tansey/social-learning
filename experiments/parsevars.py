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

def write_results_to_file(avgor, varor, avgrot, varrot, counts, filename):
	f = open(filename, 'w')
	f.write('Generation,Orientation Avg,Orientation Stdev,Velocity Avg,Velocity Stdev,Count\n')
	for i in range(len(avgor)):
		f.write('{0},{1},{2},{3},{4},{5}\n'.format(i, avgor[i], varor[i], avgrot[i], varrot[i], counts[i+1]))

def get_std_dev(list_of_nums):
	mean = sum(list_of_nums)/len(list_of_nums)
	return (sum((num - mean) **2 for num in list_of_nums) / len(list_of_nums)) **.5
	
def main():
    if len(sys.argv) != 4:
        print 'Usage: python parsevars.py <prefix> <inclusive start index> <exclusive end index>'
        return
    lines = []
    num_gens = 1000
    name = sys.argv[1] + '_results'
    for i in range(num_gens + 1):
    	lines.append([])
    start_offset = int(sys.argv[2])
    end_offset = int(sys.argv[3])
    # Load each line into the arrays
    for i in range(start_offset, end_offset):
    	append_lines(lines, name + str(i) + '_diversity_after.csv')
    avg_or = []
    var_or = []
    avg_rot =[]
    var_rot = []
    counts = []
    counts.append([])
    # Process each generation
    for i in range(1,num_gens):
    	counts.append(len(lines[i]))
    	if counts[i] > 0:
		avg_or.append(sum(float(x.split(',')[1]) for x in lines[i]) /counts[i])
		var_or.append(get_std_dev([float(x.split(',')[1]) for x in lines[i]]))
		avg_rot.append(sum(float(x.split(',')[2]) for x in lines[i]) / counts[i])
		var_rot.append(get_std_dev([float(x.split(',')[2]) for x in lines[i]]))
    write_results_to_file(avg_or, var_or, avg_rot, var_rot, counts, 'diversity_after_average_' + name + '.csv')


if __name__ == "__main__":
    main()
