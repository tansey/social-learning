import sys

def main(args):
  init_dir =args[1]
  if init_dir[-1] != '/':
    init_dir = init_dir + '/'
  executable= args[2]
  log = init_dir+num+'.log'
  out = init_dir+num+'.out'
  err = init_dir+num+".err"
  executable = '/u/elie/condorapp/CondorApp.exe'
  job_num = args[3]

  f = open("condor_job"+job_num+".txt", 'w')

  f.write("universe = vanilla\n")
  f.write("Initialdir = %s \n" % (init_dir))
  f.write("Executable = %s \n" % (executable))
  f.write("+Group   = \"GRAD\"\n")
  f.write("+Project = \"AI_ROBOTICS\"\n")
  f.write("ProjectDescription = \"Running a great many experiments to update social learning statistics\"\n")
  f.write("Log = %s" % log)
  f.write("Arguments = %s %s \n" %(executable, init_dir,) )

  f.write("Out = %s\n" % out)
  f.write("Err = %s\n" % err)
  f.write("Queue 1")

  f.close()

main(sys.argv)