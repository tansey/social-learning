def get_values(line):
    
    return [int(n) for n in line.split(',')]

def get_avg_values_per_timestep(buncha_lines, j):
    for i in range(20):
        this_step = [line for line in buncha_lines if line[0] == i]
        print averages(this_step, j)

def averages(step, i):
    top = [line[i + 1] for line in step]
    bottom = len(step)
    return sum(top) / float(bottom)

def get_vals_from_file(filename):
    f = open(filename, 'r')
    for line in f:
        if line[0] != 'g':
            line = get_values(line)
            yield line

def main():
    neural = []
    social = []
    neural_name = "neural_results"
    social_name = "social_results"
    for i in range(5):
        for line in get_vals_from_file(neural_name + str(i) + '.txt'):
            neural.append(line)
        for line in get_vals_from_file(social_name + str(i) + '.txt'):
            social.append(line)
    print "neural"
    get_avg_values_per_timestep(neural, 0)
    get_avg_values_per_timestep(neural, 1)
    print "social"
    get_avg_values_per_timestep(social, 0)
    get_avg_values_per_timestep(social, 1)



main()
