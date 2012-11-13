import sys

def main(args):
	init_dir =args[1]
	if init_dir[-1] != '/':
		init_dir = init_dir + '/'
	learn= args[2]
	switch = args[3]


	f = open(init_dir+"config.xml","w")

	f.write("""<?xml version="1.0" encoding="utf-8" ?>
	<Config>
	  <PopulationSize>100</PopulationSize>
	  <SpecieCount>10</SpecieCount>
	  <Activation>
	    <Scheme>CyclicFixedIters</Scheme>
	    <Iters>4</Iters>
	  </Activation>
	  <ComplexityRegulationStrategy>Absolute</ComplexityRegulationStrategy>
	  <ComplexityThreshold>50</ComplexityThreshold>
	  <AgentType>Neural</AgentType>
	  <PlantLayout>Uniform</PlantLayout>
	  <EvolutionParadigm>          %s              </EvolutionParadigm>
	  <Description>
	    Foraging Evolution
	    Fitness is the sum of the scores of the plants an individual ate.
	  </Description>
	  <TimeStepsPerGeneration>1000</TimeStepsPerGeneration>
	  <PlantSpecies>
	    <Plant>
	      <Name>Poison_1</Name>
	      <Radius>5</Radius>
	      <Reward>-100</Reward>
	      <Count>20</Count>
	    </Plant>
	    <Plant>
	      <Name>Poison_2</Name>
	      <Radius>5</Radius>
	      <Reward>-50</Reward>
	      <Count>20</Count>
	    </Plant>
	    <Plant>
	      <Name>Neutral_1</Name>
	      <Radius>5</Radius>
	      <Reward>0</Reward>
	      <Count>20</Count>
	    </Plant>
	    <Plant>
	      <Name>Nutritious_1</Name>
	      <Radius>5</Radius>
	      <Reward>100</Reward>
	      <Count>20</Count>
	    </Plant>
	    <Plant>
	      <Name>Nutritious_2</Name>
	      <Radius>5</Radius>
	      <Reward>50</Reward>
	      <Count>20</Count>
	    </Plant>
	  </PlantSpecies>
	  <Predators>3</Predators>
	  <WorldWidth>500</WorldWidth>
	  <WorldHeight>500</WorldHeight>
	  <AgentHorizon>100</AgentHorizon>
	  <AgentsNavigate>True</AgentsNavigate>
	  <AgentsHide>True</AgentsHide>
	  <StepReward>1</StepReward>
	</Config>""" %(learn))

main(sys.argv)