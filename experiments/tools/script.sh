#!/bin/bash
gens=250;
learns=("Neuro" "Social" "Angry");
switchs=(0 1);
runs=({1..2});
job=0
root="/u/elie/condor_experiments/"

root="./"
for run in "${runs[@]}"
do
	for learn in "${learns[@]}"
	do
		for switch in "${switchs[@]}"
			do
				mkdir $root$learn"_"$switch"_"$run
				python write_xml.py    $root$learn"_"$switch"_"$run $learn $switch
				python write_condor.py $root$learn"_"$switch"_"$run/ /u/elie/condorapp/CondorApp.exe $job $gens $run
				job=`expr $job + 1` 

		done
	done
done