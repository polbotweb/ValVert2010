# ---- names for 2010 signage: ONLY OSM name tags (ODbL). Nodes with id >= 1e10 (mapped 2022+) skipped as likely post-2010 businesses.
from PIL import ImageDraw as _ID, ImageFont as _IFt
_X=lambda n:((n['lon']-6.107796675787339)*77484.85,(n['lat']-45.88915585177747)*111132.9)
_ok=lambda q:-531<q[0]<963 and -353<q[1]<353
_SKIPW={62584900,62590936,62593722,62587176,376970874,822961300}   # GF/Lidl/Aldi have own logos; residences: no shop sign; Carrefour Market way id is recent
_BADA={None,'shelter','bench','bicycle_rental','car_sharing','letter_box','public_bookcase','parcel_locker','charging_station','recycling','vending_machine','bicycle_repair_station'}
NAMEDW={}; NAMEDN=[]; SKIPPED_NEW=[]
for e in _osm:
    t=e.get('tags',{}); nm=t.get('name')
    if not nm: continue
    if e['type']=='way' and 'building' in t and e['id'] not in _SKIPW and all(n in _N for n in e['nodes']):
        if _ok(_X(_N[e['nodes'][0]])): NAMEDW[e['id']]=nm
    elif e['type']=='node' and (t.get('shop') or t.get('amenity') not in _BADA or t.get('craft') or t.get('office')) and _ok(_X(e)):
        if e['id']>=10**10: SKIPPED_NEW.append(nm); continue
        NAMEDN.append((nm,np.array(_X(e))))
_all=sorted(set(list(NAMEDW.values())+[n for n,_ in NAMEDN]))
SROW={n:i for i,n in enumerate(_all)}; _RH=48; _AH=1
while _AH<len(_all)*_RH: _AH*=2
_im=_Im.new('RGB',(2048,_AH),(250,250,248)); _d=_ID.Draw(_im); _f=_IFt.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',34)
SASP={}
_pal=[((20,60,140),(255,255,255)),((180,25,30),(255,255,255)),((250,250,248),(30,60,120)),((30,110,60),(255,255,255)),((250,250,248),(160,20,30))]
for n,i in SROW.items():
    bg,fg=_pal[hash(n)%5 if False else sum(map(ord,n))%5]; y=i*_RH; w=min(2040,int(_d.textlength(n,font=_f))+30)
    _d.rectangle((0,y,w,y+_RH-1),fill=bg); _d.text((15,y+_RH//2),n,fill=fg,font=_f,anchor='lm'); SASP[n]=(w/2048,w/_RH)
_im.save('/workspace/ValVertGame/Assets/ValVert/Gen/sign_names.png')
CLAD={wid:['cladding','clad_white','clad_grey','clad_beige'][sum(map(ord,nm))%4] for wid,nm in NAMEDW.items()}
print('NAMES atlas',len(_all),'ways',len(NAMEDW),'nodes',len(NAMEDN),'skipped post-2010?',len(SKIPPED_NEW))
