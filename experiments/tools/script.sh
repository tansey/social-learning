#!/bin/bash
gens=500;
learns=("SubcultureRewardsAndPolling" "EveryoneRewardsAndPolling" "EveryoneRewards" "EveryonePolling" "Neural" "SubcultureRewards" "SubculturePolling");
switchs=(.001 .01 .1 .5 0 1 10 50);
evos=("Lamarkian" "Darwinian")
visibles=("True" "False")
runs=({1..5});
job=0
root="/u/elie/condor_experiments/"

for run in "${runs[@]}"
do
	for learn in "${learns[@]}"
	do
		for switch in "${switchs[@]}"
		do
		    for evo in "${evos[@]}"
		    do
			for visible in "${visibles[@]}"
			do
			        exptdir=$root$learn"_"$switch"_"$evo"_"$visible$run
			        mkdir $exptdir
				python write_xml.py    $exptdir $learn $switch $evo $visible
				python write_condor.py $exptdir /u/elie/social-learning/condor_build/CondorApp.exe $job $gens $run
				/lusr/opt/condor/bin/condor_submit "condor_job"$job".txt"
				job=`expr $job + 1` 
			done
		    done
		done
	done
done