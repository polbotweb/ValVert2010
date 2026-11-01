import json
from PIL import Image, ImageDraw, ImageFont
S,N,W,E=45.8872,45.8922,6.1075,6.1170
im=Image.open('sk_ORTHOIMAGERY.ORTHOPHOTOS2006-2010.jpg').convert('RGB')
im=Image.blend(im,Image.new('RGB',im.size,(255,255,255)),0.15)
Wd,Ht=im.size
def px(lat,lon): return ((lon-W)/(E-W)*Wd,(N-lat)/(N-S)*Ht)
d=json.load(open('big.json'))
nodes={e['id']:(e['lat'],e['lon']) for e in d['elements'] if e['type']=='node'}
dr=ImageDraw.Draw(im)
try:
  F=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',22); Fs=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',17)
except: F=Fs=ImageFont.load_default()
roadlab={}
for e in d['elements']:
  if e['type']!='way': continue
  t=e.get('tags',{}); pts=[px(*nodes[n]) for n in e['nodes'] if n in nodes]
  if len(pts)<2: continue
  if t.get('water')=='basin' or t.get('natural')=='water':
    dr.polygon(pts,outline=(0,140,255),fill=None); dr.line(pts,fill=(0,140,255),width=4)
  elif t.get('waterway'):
    dr.line(pts,fill=(0,170,255),width=4)
  elif t.get('highway') in ('primary','secondary','tertiary','residential','unclassified','service','living_street'):
    dr.line(pts,fill=(255,230,0),width=5 if t['highway']!='service' else 2)
    n=t.get('name')
    if n in('Avenue des Trois Fontaines','Avenue Montaigne','Rue du Val Vert','Rue de Sansy','Avenue de Barral','Avenue de Loverchy'):
      L=sum(abs(pts[i+1][0]-pts[i][0])+abs(pts[i+1][1]-pts[i][1]) for i in range(len(pts)-1))
      if L>roadlab.get(n,(0,))[0]: roadlab[n]=(L,pts[len(pts)//2])
  elif t.get('highway') in ('footway','path','steps','pedestrian','cycleway'):
    dr.line(pts,fill=(255,255,255),width=2)
  elif t.get('shop')=='supermarket' or t.get('amenity')=='school':
    dr.polygon(pts,outline=(255,0,0)); dr.line(pts+[pts[0]],fill=(255,40,40),width=4)
def lab(txt,lat,lon,col=(255,40,40),f=None,dx=10,dy=-10):
  x,y=px(lat,lon); f=f or F
  dr.ellipse([x-7,y-7,x+7,y+7],fill=col,outline='black')
  b=dr.textbbox((x+dx,y+dy),txt,font=f); dr.rectangle([b[0]-4,b[1]-3,b[2]+4,b[3]+3],fill=(0,0,0))
  dr.text((x+dx,y+dy),txt,font=f,fill=col)
for n,(L,(x,y)) in roadlab.items():
  b=dr.textbbox((x,y),n,font=Fs); dr.rectangle([b[0]-3,b[1]-2,b[2]+3,b[3]+2],fill=(60,60,0)); dr.text((x,y),n,font=Fs,fill=(255,240,80))
lab('ALDI',45.88968,6.11355)
lab('GRAND FRAIS',45.89046,6.11372)
lab('LIDL',45.88864,6.11492)
lab('ECOLE BARRAL',45.89151,6.11213)
lab('Arret bus Trois Fontaines',45.88917,6.11386,col=(255,150,0),f=Fs,dy=12)
lab('BASSIN (Deversoir)',45.88907,6.11256,col=(0,200,255))
# compass
dr.polygon([(60,40),(45,90),(75,90)],fill='white',outline='black'); dr.text((52,95),'N',font=F,fill='white')
dr.text((20,Ht-40),'Photo aerienne IGN 2006-2010 + routes OpenStreetMap',font=Fs,fill='white')
im.save('/workspace/valvert/plan_piranha.png')
print(im.size)
