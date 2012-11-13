#!/bin/bash
gens=500;
learns=("Neural" "SubcultureRewards" "SubculturePolling");
switchs=(0 1 10 50);
runs=({11..50});
job=0
root="/u/elie/condor_experiments/"

for run in "${runs[@]}"
do
	for learn in "${learns[@]}"
	do
		for switch in "${switchs[@]}"
			do
				mkdir $root$learn"_"$switch"_"$run
				python write_xml.py    $root$learn"_"$switch"_"$run $learn $switch
				python write_condor.py $root$learn"_"$switch"_"$run/ /u/elie/social-learning/condor_build/CondorApp.exe $job $gens $run
				/lusr/opt/condor/bin/condor_submit "condor_job"$job".txt"
				job=`expr $job + 1` 

		done
	done
done