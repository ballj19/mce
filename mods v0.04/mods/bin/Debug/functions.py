def build_imap():
	imap = [[0 for x in range(8)] for y in range(16)]
	
	imap[0]  = ['CTL','WLD','HLD','HLI','LLI','EQI','EDS','CWI']
	imap[1]  = ['CD5','CD4','CD3','CTF','MGS','EMT','CCC','DLI']
	imap[2]  = ['LSR','ESS','PTI','EMSC','AXR','CNP','EMSH','PFGE']
	imap[3]  = ['NSI','DNI','UPI','ATS','GOV','SAFH','SAFC','STOP']
	imap[4]  = ['FRON','EPR','ECRN','BPS','FRBYP','UDF','ALT','HOSP']
	imap[5]  = ['FRON2','HCRI','ASTP','ABI','SABI','DLS','DCL','GSM']
	imap[6]  = ['CTST','X','SWG','FRAON','RGSR','RGS','DHLDR','DHLDF']
	imap[7]  = ['PREOP','DTS','REO','FP2S','RDLSR','FRAA','BSI','OVL']

	imap[8]  = ['ECS1','DSTIR','EPI','LPK','HIND','DSTIF','PRIS','LWB']
	imap[9]  = ['FCCC','TEST','CPOIR','CPCIR','EDTLS','FCOFF','CPOIF','CPCIF']
	imap[10] = ['ASP1','CRO','FRHTW','FRMR','DRON','*MPS3','*MPS2','*MPS1']
	imap[11] = ['DPMR','XXX','FRSA','FRSM','DPMF','INSL','ARST','ASP2']
	imap[12] = ['XXX','RBAB','R2AB','RINAX','XXX','BAB','2AB','INA']
	imap[13] = ['XXX','XXX','XXX','XXX','XXX','XXX','XXX','XXX']
	imap[14] = ['XXX','XXX','XXX','XXX','XXX','XXX','XXX','XXX']
	imap[15] = ['XXX','XXX','XXX','XXX','XXX','XXX','XXX','XXX']
	
	return imap
	
def hex_to_bin(text):
	return bin(int(remove_suffix(remove_prefix(text,"DB"),"H")[-2:],16))[2:].zfill(8)

def is_hex(text):
	if "H" not in text:
		return False
	else:
		return True

def remove_prefix(text, prefix):
    if text.startswith(prefix):
        return text[len(prefix):]
    return text
	
def remove_suffix(text, suffix):
	if text.endswith(suffix):
		return text[:-len(suffix)]
	return text

def remove_comments(text):
		try:
			comment_index = text.index(";")
			return text[:comment_index].strip()
		except:
			return text

def bottom_floor(content):
	bottom_index = content.index("BOTTOM:")
	bottom_floor = remove_prefix(content[bottom_index + 1],"DB")
	
	if is_hex(bottom_floor):
		return "Bottom Landing: " + bottom_floor + " (" + str(int(remove_suffix(bottom_floor,"H"),16)+1) + ")"
	else:
		return "Bottom Landing: " + str(hex(int(bottom_floor)).split('x')[-1]) + "H (" + str(int(bottom_floor) + 1) + ")"

def top_floor(content):
	bottom_index = content.index("BOTTOM:")	
	top_floor = remove_prefix(content[bottom_index + 2],"DB")
	
	if is_hex(top_floor):
		return "Top Landing: " + top_floor + " (" + str(int(remove_suffix(top_floor,"H"),16)+1) + ")"
	else:
		return "Top Landing: " + str(hex(int(top_floor)).split('x')[-1]) + "H (" + str(int(top_floor) + 1) + ")"

def top_floor_sim(content):
	bottom_index = content.index("BOTTOM:")	
	top_floor = remove_prefix(content[bottom_index + 2],"DB")
		
	if is_hex(top_floor):
		return int(remove_suffix(top_floor,"H"),16)+1
	else:
		return int(top_floor) + 1
		
def inputs(content):
	input_x = 0
	input_y = 7

	inputs = [[0 for x in range(8)] for y in range(8)]
	imap = build_imap()
	
	input_index = content.index("IOINPE:")
	input2_index = content.index("IOINPE2:")

	for x in range(8):
		imap_index = input_index + x + 1
		imap_binary = hex_to_bin(content[imap_index])
		
		for y in range(8):
			if imap_binary[7-y] == '1':
				inputs[input_x][input_y] = imap[x][7-y]
				input_y -= 1
				
				if input_y == -1:
					input_x += 1
					input_y = 7
					
	for x in range(8):
		imap_index = input2_index + x + 1
		imap_binary = hex_to_bin(content[imap_index])
		
		for y in range(8):
			if imap_binary[7-y] == '1':
				inputs[input_x][input_y] = imap[x+8][7-y]
				input_y -= 1
				
				if input_y == -1:
					input_x += 1
					input_y = 7
					
	for x in range(8):
		for y in range(8):
			if inputs[x][y] == 0:
				print('XXX',end='')
			else:
				print(inputs[x][y],end='')
			if y == 3:
				print('--||--',end='')
			elif y == 7:
				print()
			else:
				print('--',end='')

def outputs(content):
	output_x = 0
	output_y = 7

	omap = [[0 for x in range(8)] for y in range(16)]
	outputs = [[0 for x in range(8)] for y in range(8)]

	omap[0]  = ['MLT','FRC','CD','HCR','FRM','ISV','HLW','EQIND']
	omap[1]  = ['MISV','HEO/EMSIH','DSHT','HWI/EMSIC','TOS','PH1','DO16','DO8']
	omap[2]  = ['DO4','DO2','DO1','ISRT','ONEP','SAF','LLW','CCUCDC']
	omap[3]  = ['HCP','HCA','SLV','HOSPH2','EQA','900','MGRTN','DO32']
	omap[4]  = ['SEC','DLW','CSB','HSEL','CSEO','EPR','DSHTR','EMSB']
	omap[5]  = ['DMD','DMU','LUP','ABZ','UPO','DNO','FLO','CFLT']
	omap[6]  = ['MFIR','AFIR','FSO','DLOB','WLDI','INDFRC','CTLDO','RSR']
	omap[7]  = ['RD1','RD2','RD3','ALTO','CCHCR','OLW','EFG','HCT']

	omap[8]  = ['CGEUR','CGEDR','CGEU','CGED','EQSTP','DHEND','DHENDR','PRISO']
	omap[9]  = ['DHLDOF','DHLDOR','EP1','ATSF','X','X','X','FSVC']
	omap[10]  = ['FWL','TDISL','DISL','DISB','XXX','XXX','XXX','XXX']
	omap[11]  = ['XXX','XXX','XXX','XXX','XXX','XXX','XXX','XXX']
	omap[12]  = ['XXX','XXX','XXX','XXX','XXX','XXX','XXX','XXX']
	omap[13]  = ['XXX','XXX','XXX','XXX','XXX','XXX','XXX','XXX']
	omap[14]  = ['XXX','XXX','XXX','XXX','XXX','XXX','XXX','XXX']
	omap[15]  = ['XXX','XXX','XXX','XXX','XXX','XXX','XXX','XXX']

	
	output_index = content.index("IOOUTE:")
	output2_index = content.index("IOOUTE2:")

	for x in range(8):
		omap_index = output_index + x + 1
		omap_binary = hex_to_bin(content[omap_index])
		
		for y in range(8):
			if omap_binary[7-y] == '1':
				outputs[output_x][output_y] = omap[x][7-y]
				output_y -= 1
				
				if output_y == -1:
					output_x += 1
					output_y = 7
					
	for x in range(8):
		omap_index = output2_index + x + 1
		omap_binary = hex_to_bin(content[omap_index])
		
		for y in range(8):
			if omap_binary[7-y] == '1':
				outputs[output_x][output_y] = omap[x+8][7-y]
				output_y -= 1
				
				if output_y == -1:
					output_x += 1
					output_y = 7
					
	for x in range(8):
		for y in range(8):
			if outputs[x][y] == 0:
				print('XXX',end='')
			else:
				print(outputs[x][y],end='')
			if y == 3:
				print('--||--',end='')
			elif y == 7:
				print()
			else:
				print('--',end='')
				
def sim_inputs(content):
	input_x = 0
	input_y = 7

	inputs = [[0 for x in range(8)] for y in range(8)]

	imap = build_imap()
	
	input_index = content.index("IOINPE:")
	input2_index = content.index("IOINPE2:")

	for x in range(8):
		imap_index = input_index + x + 1
		imap_binary = hex_to_bin(content[imap_index])
		
		for y in range(8):
			if imap_binary[7-y] == '1':
				inputs[input_x][input_y] = imap[x][7-y]
				input_y -= 1
				
				if input_y == -1:
					input_x += 1
					input_y = 7
					
	for x in range(8):
		imap_index = input2_index + x + 1
		imap_binary = hex_to_bin(content[imap_index])
		
		for y in range(8):
			if imap_binary[7-y] == '1':
				inputs[input_x][input_y] = imap[x+8][7-y]
				input_y -= 1
				
				if input_y == -1:
					input_x += 1
					input_y = 7
	
	return inputs