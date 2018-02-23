from functions import *
import sys

global file
global index
index = 0

number_of_landings_index = 661
landing_config_index = 664
ccelig_index = 858
fhcelig_index = 988
rhcelig_index = 1118
hospelig_index = 1248
iox_index = 1555
i4o_index = 1558
aiox_index = 1561
spare1_index = 7878
eof_index = 8753

def write_filler(sim_content,index,end_index):
	for x in range(index,end_index):
		line_write(sim_content[x])

def write_top_landing(asm_content):
	bottom_index = asm_content.index("BOTTOM:")	
	top_floor = remove_prefix(asm_content[bottom_index + 2],"DB")
	
	if is_hex(top_floor):
		line_write('Value= ' + str(int(remove_suffix(top_floor,"H"),16)+1))
	else:
		line_write('Value= ' + str(int(top_floor) + 1))

def write_landing_config(asm_content):
	number_of_landings = top_floor_sim(asm_content)
	for x in range(number_of_landings):
		landing = str(x + 1)
		line_write('Value Height ' + landing + ' = 10')
		line_write('Value ' + landing + ' F = True')
		line_write('Value ' + landing + ' R = False')

def write_ccelig(asm_content):
	number_of_landings = top_floor_sim(asm_content)
	for x in range(number_of_landings):
		landing = str(x + 1)
		line_write('Value ' + landing + ' F = True')
		line_write('Value ' + landing + ' R = False')		

def write_inputs(sim_inputs):
	spare_number = 1
	for x in range(8):
		for y in range(8):
			if sim_inputs[x][7-y] == 0:
				break
			else:
				line_write('[SpareSwComboBox' + str(spare_number))
				line_write('Value= ' + sim_inputs[x][7-y])
				line_write('')
				spare_number += 1
				
def write_iox_boards(asm_content):
	iox_boards_index = asm_content.index("LOBBY:") + 40
	line_write('Value= ' + str(remove_suffix(remove_prefix(asm_content[iox_boards_index],"DB"),"H")[-1:]))

def write_i4o_boards(asm_content):
	iox_boards_index = asm_content.index("LOBBY:") + 40
	line_write('Value= ' + str(remove_suffix(remove_prefix(asm_content[iox_boards_index],"DB"),"H")[-2:-1]))

def line_write(text):
	global index
	global file
	file.write(text + '\n')
	index += 1
	
filestring = "G:/Software/Product/" + str(sys.argv[2]) + "/" + str(sys.argv[1]) + ".asm"

with open(filestring,"r") as file:
	asm_content = file.readlines()
asm_content = [x.strip() for x in asm_content]
for x in range(len(asm_content)):
	asm_content[x] = remove_comments(asm_content[x])
asm_content = list(filter(None,asm_content))

with open("sim_base.sdf","r") as f:
	sim_content = f.readlines()
sim_content = [x.strip() for x in sim_content]

sim_inputs = sim_inputs(asm_content)


filewritestring = "C:/Simulator/" + str(sys.argv[1]) + ".sdf"
file = open(filewritestring,"w")

write_filler(sim_content,index,number_of_landings_index)
write_top_landing(asm_content)
write_filler(sim_content,index,landing_config_index)
write_landing_config(asm_content)
write_filler(sim_content,index,ccelig_index)
write_ccelig(asm_content)
write_filler(sim_content,index,iox_index)
write_iox_boards(asm_content)
write_filler(sim_content,index,i4o_index)
write_i4o_boards(asm_content)
write_filler(sim_content,index,spare1_index)
write_inputs(sim_inputs)
write_filler(sim_content,index,eof_index)
