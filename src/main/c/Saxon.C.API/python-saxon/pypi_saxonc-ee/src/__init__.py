import os

if os.getenv('SAXONC_HOME') != None and "saxonc_home" in os.getenv('SAXONC_HOME'):
	pass
else:
	main_dir = os.path.dirname(os.path.abspath(__file__))
	saxonc_home = "".join([main_dir, "/saxonc_home"])
	os.environ['SAXONC_HOME'] = saxonc_home

if os.getenv('LD_LIBRARY_PATH') != None and "saxonc_home" in os.getenv('LD_LIBRARY_PATH'):
	pass
else:
	main_dir = os.path.dirname(os.path.abspath(__file__))
	saxonc_home = "".join([main_dir, "/saxonc_home"])
	os.environ['LD_LIBRARY_PATH'] = saxonc_home


from saxonc import *