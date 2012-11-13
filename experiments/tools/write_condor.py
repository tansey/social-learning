import sys

def main(args):
  init_dir =args[1]
  if init_dir[-1] != '/':
    init_dir = init_dir + '/'
  executable= args[2]
  job_num = args[3]
  max_gens = args[4]
  offset = args[5]
  mono = "/lusr/opt/mono-2.10.8/bin/mono"
  log = init_dir+job_num+'.log'
  out = init_dir+job_num+'.out'
  err = init_dir+job_num+".err"
  executable = '/u/elie/social-learning/condor_build/CondorApp.exe'


  f = open("condor_job"+job_num+".txt", 'w')

  f.write("universe = vanilla\n")
  f.write("Initialdir = %s \n" % (init_dir))
  f.write("Executable = %s \n" % (mono))
  f.write("+Group   = \"GRAD\"\n")
  f.write("+Project = \"AI_ROBOTICS\"\n")
  f.write("+ProjectDescription = \"Running a great many experiments to update social learning statistics\"\n")
  f.write("Log = %s\n" % log)
  f.write("Arguments = %s %s %s %s\n" %(executable, init_dir, max_gens, offset) )

  f.write("Output = %s\n" % out)
  f.write("Error = %s\n" % err)
  f.write("Queue 1")

  f.close()

main(sys.argv)
