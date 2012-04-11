######## Config Parameters #########

#--Define data file directory:
dir <- "C:/Users/Wesley/Downloads"

fileone <- "test.csv"
titleone <- "Social Only"

filetwo <- "test2.csv"
titletwo <- "Evolution Only"

#filethree <- "simple_neural_only.csv"
#titlethree <- "Neuroevolution"

#--Define axis labels:
xlabel <- "Generations"
ylabel <- "Champion Fitness"

outfile <- "egalitarian_vs_student_teacher_complex.pdf"

#####################################

# Read in all the data
SL <- read.csv(paste(dir, fileone, sep="/"), header=TRUE)
NE <- read.csv(paste(dir, filetwo, sep="/"), header=TRUE)
#TT <- read.csv(paste(dir, filethree, sep="/"), header=TRUE)

#--Load extra library:
## if not already installed, then run:
# install.packages("ggplot2")
require(ggplot2)

#--Combine datasets into a single data frame:
SL$type <- titleone
NE$type <- titletwo
#TT$type <- titlethree
#A <- rbind(SL, NE, TT)
A <- rbind(SL, NE)

p <- ggplot(data=A, aes(x=Gen, y=Best, ymin=Best.down, ymax=Best.up, fill=type, linetype=type)) + 
 geom_line() + 
 geom_ribbon(alpha=0.5) + 
 scale_linetype_manual(values=c("solid", "dashed", "dotted")) +
 xlab(xlabel) + 
 ylab(ylabel) 
 #scale_y_continuous(limits = c(500, 3000), breaks=seq(500, 3000, 500))

 p <- p + opts(
    panel.background = theme_rect(fill = "transparent",colour = NA),
    panel.border = theme_rect(colour = 'black', fill = 'transparent', size = 2, linetype='solid'),
    panel.grid.minor = theme_blank(), 
    panel.grid.major = theme_blank(),
    axis.line = theme_segment(),
    plot.background = theme_rect(fill = "transparent",colour = NA),
    legend.position = c(0.85, 0.75),
    legend.title = theme_blank(),
    legend.text = theme_text(colour = 'black', face='bold'),
    axis.text.x = theme_text(colour = 'black', face='bold'),
    axis.text.y = theme_text(colour = 'black', face='bold'),
    axis.title.x = theme_text(colour = 'black', face = 'bold'),
    axis.title.y = theme_text(colour = 'black', face = 'bold', angle = 90),
    axis.ticks = theme_segment(colour = 'black', size=1, linetype='solid')
)

ggsave(p, file=paste(dir, outfile, sep="/"), width=8, height=4.5)