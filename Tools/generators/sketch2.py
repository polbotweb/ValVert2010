import json
from PIL import Image, ImageDraw, ImageFont
S,N,W,E=45.8881,45.8903,6.1108,6.1152
im=Image.open('zoom2010.jpg').convert('RGB'); Wd,Ht=im.size
def px(la,lo): return ((lo-W)/(E-W)*Wd,(N-la)/(N-S)*Ht)
d=json.load(open('big.json'))
nodes={e['id']:(e['lat'],e['lon']) for e in d['elements'] if e['type']=='node'}
dr=ImageDraw.Draw(im)
F=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',26)
Fs=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',20)
for e in d['elements']:
  if e['type']!='way': continue
  t=e.get('tags',{}); pts=[px(*nodes[n]) for n in e['nodes'] if n in nodes]
  if len(pts)<2: continue
  h=t.get('highway')
  if h in('tertiary','residential','unclassified','service'): dr.line(pts,fill=(255,225,0),width=6 if h!='service' else 3)
  elif h in('path','footway','cycleway','pedestrian'): dr.line(pts,fill=(255,255,255),width=4)
  elif h=='steps': dr.line(pts,fill=(255,0,255),width=9)
  elif t.get('waterway'): dr.line(pts,fill=(0,170,255),width=6)
  elif t.get('water')=='basin': dr.line(pts+[pts[0]],fill=(0,120,255),width=4)
  elif t.get('waterway')=='dam' or t.get('waterway')=='weir': dr.line(pts,fill=(255,60,60),width=8)
  elif t.get('shop')=='supermarket': dr.line(pts+[pts[0]],fill=(255,50,50),width=4)
  if t.get('waterway') in('dam','weir'): dr.line(pts,fill=(255,60,60),width=10)
def lab(num,txt,la,lo,col,dx=14,dy=-14):
  x,y=px(la,lo)
  dr.ellipse([x-18,y-18,x+18,y+18],fill=col,outline='black',width=3)
  dr.text((x-8 if num<10 else x-15,y-15),str(num),font=F,fill='black')
  b=dr.textbbox((x+22+dx,y+dy),txt,font=Fs); dr.rectangle([b[0]-5,b[1]-4,b[2]+5,b[3]+4],fill=(0,0,0))
  dr.text((x+22+dx,y+dy),txt,font=Fs,fill=col)
lab(1,'QUEUE du poisson (aileron beton + tube vert)',45.88930,6.11372,(80,255,80),dx=-560,dy=-60)
lab(2,'ESCALIER qui descend vers le deversoir',45.88915,6.11350,(255,0,255),dx=-470,dy=30)
lab(3,'DEVERSOIR / barrage (arche) = TETE ?',45.88922,6.11196,(255,80,80),dx=0,dy=-55)
lab(4,'Chemin qui fait le TOUR',45.88875,6.11207,(255,255,255),dx=0,dy=20)
lab(5,'Aldi',45.88968,6.11355,(255,80,80),dx=0)
lab(6,'Arret bus Trois Fontaines',45.88917,6.11386,(255,170,0),dx=0,dy=10)
lab(7,'Avenue des Trois Fontaines',45.88990,6.11440,(255,225,0),dx=-330,dy=40)
lab(8,'Ruisseau',45.88905,6.11270,(0,190,255),dx=0,dy=-50)
dr.polygon([(1340,30),(1325,80),(1355,80)],fill='white',outline='black'); dr.text((1330,85),'N',font=F,fill='white')
im.save('/workspace/valvert/plan_piranha_zoom.png')
