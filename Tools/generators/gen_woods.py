import json,numpy as np,math
from scipy import ndimage as nd
from PIL import Image,ImageDraw
R=2049; SX=1064/2048; SZ=709.293/2048
h0=np.fromfile('/workspace/valvert/clean/heightmap_2049_smooth.raw','<u2').reshape(R,R)/65535*80.056+0.002
GX=-532+np.arange(R)*SX; GZ=-354.646+np.arange(R)*SZ
def Hs(a,x,z):
    fx=(x+532)/SX; fz=(z+354.646)/SZ; return nd.map_coordinates(a,[np.atleast_1d(fz),np.atleast_1d(fx)],order=1,mode='nearest')
h=h0.copy()
# IGN RGE ALTI check (geopf API, 4 m probe gen/offset_probe_4m.npy): NE block (x~100-430, z>~184) of the source DEM sits +4.2 m above the IGN MNT,
# leaving a false 4 m cliff across the #9/#11/#13 access road (OSM 1378070581) -> remove the offset so the ground matches the MNT
_h0raw=h0.copy(); _d=np.load('/workspace/v3/gen/offset_probe_4m.npy'); _m=np.abs(_d-4.2)<0.9
_m=nd.binary_closing(_m,iterations=2); _m=nd.binary_fill_holes(_m); _m=nd.binary_opening(_m,iterations=2)
_l,_n=nd.label(_m); _m=_l==(1+np.argmax(nd.sum(_m,_l,range(1,_n+1))))
_off=float(np.median(_d[_m&(np.abs(_d-4.2)<0.9)]))
_mf=nd.map_coordinates(_m.astype(float),[ (GZ[:,None]*0+GZ[:,None]-120)/4+0*GX[None,:], (GX[None,:]-100)/4+0*GZ[:,None]],order=1,mode='constant',cval=0.0)
_M=_mf>0.5; h=h-_off*_M
_B=nd.binary_dilation(_M,iterations=8)&~nd.binary_erosion(_M,iterations=8)   # +-4 m seam band: harmonic fill (no step, no dip)
_ys,_xs=np.where(_B); _y0,_y1,_x0,_x1=_ys.min()-1,_ys.max()+2,_xs.min()-1,_xs.max()+2
_s=h[_y0:_y1,_x0:_x1].copy(); _b=_B[_y0:_y1,_x0:_x1]
for _i in range(1500):
    _a=0.25*(np.roll(_s,1,0)+np.roll(_s,-1,0)+np.roll(_s,1,1)+np.roll(_s,-1,1)); _s[_b]=_a[_b]
h[_y0:_y1,_x0:_x1]=_s; h0=h.copy()
(_h0raw-h).astype('<f4').tofile('/workspace/valvert/clean/demfix_delta.raw')   # Unity drops old FBX road/path meshes by this
print('DEMFIX offset',round(_off,2),'cells',int(_m.sum()))
def dens(pts,step=1.0):
    pts=np.array(pts,float); out=[pts[0]]
    for a,b in zip(pts[:-1],pts[1:]):
        n=max(1,int(np.linalg.norm(b-a)/step))
        for t in range(1,n+1): out.append(a+(b-a)*t/n)
    return np.array(out)
def smooth(p,it=30):
    p=p.copy()
    for _ in range(it): p[1:-1]=0.25*p[:-2]+0.5*p[1:-1]+0.25*p[2:]
    return p
w={s['id']:s['pts'] for s in json.load(open('waterways.json'))}
label=np.zeros((R,R),np.uint8)
streams=[]
for sid,clip in [(628404995,None),(891572218,None)]:
    p=np.array([q for q in w[sid] if -530<q[0]<530 and -352<q[1]<352])
    p=smooth(dens(p,1.0),40)
    if sid==628404995:
        # snap to thalweg
        for it in range(3):
            t=np.gradient(p,axis=0); t/=np.linalg.norm(t,axis=1)[:,None]; nrm=np.c_[-t[:,1],t[:,0]]
            offs=np.linspace(-7,7,29); best=[]
            for i in range(len(p)):
                c=p[i]+offs[:,None]*nrm[i]; hh=Hs(h0,c[:,0],c[:,1])+np.abs(offs)*0.04
                best.append(offs[np.argmin(hh)])
            best=nd.uniform_filter1d(np.array(best),15)
            p=smooth(p+best[:,None]*nrm*0.7,20)
    p=dens(p,0.8)
    bed=Hs(h0,p[:,0],p[:,1])
    bed=nd.minimum_filter1d(bed,9)
    for i in range(1,len(bed)): bed[i]=min(bed[i],bed[i-1])  # downstream monotone
    bed=bed-0.55
    streams.append((sid,p,bed))
# carve
for sid,p,bed in streams:
    half=1.3 if sid==628404995 else 0.8
    for (x,z),b in zip(p[::2],bed[::2]):
        cut=max(0.0,float(Hs(h,x,z)[0])-b); rad=half+1.5+min(3.0,cut*1.2)
        i0=int((x-rad+532)/SX);i1=int((x+rad+532)/SX)+2;j0=int((z-rad+354.646)/SZ);j1=int((z+rad+354.646)/SZ)+2
        i0,j0=max(i0,0),max(j0,0)
        X,Z=np.meshgrid(GX[i0:i1],GZ[j0:j1]); d=np.hypot(X-x,Z-z)
        tgt=np.where(d<half,b,b+(d-half)/(rad-half)*np.maximum(h[j0:j1,i0:i1]-b,0)*1.0)
        tgt=np.where(d<half, b, b+ (np.clip((d-half)/(rad-half),0,1)**1.3)*(h[j0:j1,i0:i1]-b))
        blk=h[j0:j1,i0:i1]; h[j0:j1,i0:i1]=np.where(d<rad,np.minimum(blk,tgt),blk)
        lb=label[j0:j1,i0:i1]; lb[(d<half+0.9)&(lb==0)]=4
# pond, terraces, riprap labels
def c2u(cx,cy): return (280+cx/6, 40.146-cy/6)
pond=[c2u(*q) for q in [(410,300),(440,250),(500,205),(560,190),(620,200),(700,215),(800,225),(840,240),(860,280),(850,330),(800,370),(760,350),(700,330),(620,320),(540,330),(480,370),(445,400),(420,370)]]
def raster(poly):
    im=Image.new('L',(R,R)); ImageDraw.Draw(im).polygon([((x+532)/SX,(z+354.646)/SZ) for x,z in poly],fill=1); return np.asarray(im)>0
P=raster(pond)
X,Z=np.meshgrid(GX,GZ)
dout=nd.distance_transform_edt(~P)*SX
label[(dout>0)&(dout<3.2)&(label!=4)]=2
label[P]=3
C=c2u(440,190); rr=np.hypot(X-C[0],Z-C[1]); ang=np.degrees(np.arctan2(-(Z-C[1]),X-C[0]))%360
fan=(rr>23)&(rr<45)&(ang>85)&(ang<185)&~P
step=(ang-85)/7.0; frac=step-np.floor(step)
label[fan&(frac<0.32)]=1
ring=fan&(np.abs(((rr-23)/5.5)%1-0)<0.05); label[ring]=1
# dam apron carve: expose full 5.6 m face + mouth tunnel
reg=(X>=410)&(X<=424.85)&(Z>=-15)&(Z<=9.2)
w_=np.clip((X-410)/6.0,0,1); tgt=5.35
h=np.where(reg,np.minimum(h,h*(1-w_)+tgt*w_),h)
tun=(X>=424.0)&(X<=430.0)&(Z>=-4.7)&(Z<=-1.3); h=np.where(tun,5.0,h)
label[reg&(w_>0.5)]=3
# Barral court: DEM pit (12.8 m) is a building-removal artefact -> level with school yard (18.15 m)
dx=np.maximum(np.maximum(372-X,X-400),0); dz=np.maximum(np.maximum(216-Z,Z-266),0); dd=np.hypot(dx,dz)
wb=np.clip(1-dd/5.0,0,1); wb=wb*wb*(3-2*wb); h=h*(1-wb)+(18.15-_off)*wb   # yard level re-based with the IGN offset fix
# grade the two fin stairways (OSM 260433023/024): steady descent dam(12.0) -> avenue
for lo,hi in (((438.8,-11.1),(458.7,9.1)),((427.3,9.8),(454.4,17.3))):
    lo=np.array(lo);hi=np.array(hi);d=hi-lo;L=np.linalg.norm(d);d/=L
    y0=12.0; y1=float(Hs(h,*hi)[0])
    t=np.clip(((X-lo[0])*d[0]+(Z-lo[1])*d[1])/L,0,1)
    px=lo[0]+d[0]*t*L; pz=lo[1]+d[1]*t*L; dd=np.hypot(X-px,Z-pz)
    wg=np.clip(1-(dd-1.6)/3.0,0,1); wg=wg*wg*(3-2*wg); h=h*(1-wg)+(y0+(y1-y0)*t)*wg
# GF car park (OSM 62591761, user-validated): lower level = flat continuation of lot 574467194 (5.05 m) under the roof deck; no ramp/basement
dx=np.maximum(np.maximum(428.9-X,X-468.9),0); dz=np.maximum(np.maximum(108.6-Z,Z-124.6),0)
wl=np.where((X>=428.9)&(X<=468.9)&(Z>=108.6)&(Z<=124.6),1.0,np.clip(1-np.hypot(np.maximum(X-468.9,0),np.maximum(108.6-Z,0))/4.0,0,1)*((X>=428.9)&(Z<=124.6)))
h=np.where(wl>0,h*(1-wl)+np.minimum(h,5.05)*wl,h)
# GF roof deck drive-in: grade the Val Vert slope west of the deck so the asphalt runs flush onto the deck top (8.36 m) - no lip
_t=np.clip((X-398.0)/(428.9-398.0),0,1); _dz=np.maximum(np.maximum(108.6-Z,Z-124.5),0); _w=np.clip(1-_dz/3.0,0,1)*((X>=396)&(X<=428.95))
_hr=h[:,int(round((398.0+532)/(1064/2048)))][:,None]
h=np.where(_w>0,h*(1-_w)+(_hr*(1-_t)+(8.36-0.15)*_t)*_w,h)   # straight grade from the aisle (x 398) down to the deck edge
np.save('gen/label.npy',label)
open('/workspace/valvert/clean/label_v3.raw','wb').write(label.astype(np.uint8).tobytes())
hv=np.clip(np.round((h-0.002)/80.056*65535),0,65535).astype('<u2'); hv.tofile('/workspace/valvert/clean/heightmap_v3.raw')
print('carve max',(h0-h).max(),'labels',np.bincount(label.ravel()))
# ---------- meshes ----------
H=lambda x,z: float(Hs(h,x,z)[0])
meshes=[]
def addmesh(name,mat,V,T,UV): meshes.append({'name':name,'mat':mat,'v':[round(c,3) for p in V for c in p],'t':T,'uv':[round(c,3) for p in UV for c in p]})
# water ribbons
for sid,p,bed in streams:
    half=0.75 if sid==628404995 else 0.45
    V=[];T=[];UV=[]
    t=np.gradient(p,axis=0); t/=np.linalg.norm(t,axis=1)[:,None]; nrm=np.c_[-t[:,1],t[:,0]]
    for i in range(len(p)):
        y=bed[i]+0.2
        for s in (-1,1): q=p[i]+s*half*nrm[i]; V.append((q[0],y,q[1])); UV.append((q[0]/4,q[1]/4))
        if i: k=2*i; T+= [k-2,k-1,k, k-1,k+1,k]
    addmesh(f'Stream_{sid}','water',V,T,UV)
# helper solids (flat-shaded, box UV)
def quad(V,T,UV,a,b,c,d,s=1.0):
    n=np.cross(np.subtract(b,a),np.subtract(c,a)); n=n/ (np.linalg.norm(n)+1e-9); k=len(V)
    for q in (a,b,c,d):
        if abs(n[1])>0.6: UV.append((q[0]/s,q[2]/s))
        elif abs(n[0])>abs(n[2]): UV.append((q[2]/s,q[1]/s))
        else: UV.append((q[0]/s,q[1]/s))
        V.append(tuple(q))
    T+=[k,k+1,k+2,k,k+2,k+3]
def tri(V,T,UV,a,b,c,s=1.0):
    k=len(V)
    for q in (a,b,c): V.append(tuple(q)); UV.append(((q[0]+q[2])/s,q[1]/s))
    T+=[k,k+1,k+2]
def box(V,T,UV,c,size,yaw=0.0,s=1.5):
    cx,cy,cz=c; sx,sy,sz=size; ca,sa=math.cos(yaw),math.sin(yaw)
    P=lambda dx,dy,dz:(cx+dx*ca-dz*sa,cy+dy,cz+dx*sa+dz*ca)
    x,y,z=sx/2,sy,sz/2
    c8={k:P(*k) for k in [(a,b,cc) for a in(-x,x) for b in(0,y) for cc in(-z,z)]}
    g=lambda a,b,cc:c8[(a,b,cc)]
    quad(V,T,UV,g(-x,0,-z),g(-x,y,-z),g(x,y,-z),g(x,0,-z),s)
    quad(V,T,UV,g(x,0,z),g(x,y,z),g(-x,y,z),g(-x,0,z),s)
    quad(V,T,UV,g(-x,0,z),g(-x,y,z),g(-x,y,-z),g(-x,0,-z),s)
    quad(V,T,UV,g(x,0,-z),g(x,y,-z),g(x,y,z),g(x,0,z),s)
    quad(V,T,UV,g(-x,y,-z),g(-x,y,z),g(x,y,z),g(x,y,-z),s)
def tube(V,T,UV,path,r=0.06,n=8):
    path=np.array(path); k0=len(V)
    for i,p in enumerate(path):
        t=path[min(i+1,len(path)-1)]-path[max(i-1,0)]; t=t/np.linalg.norm(t)
        a=np.cross(t,[0,1,0]); a=a/np.linalg.norm(a) if np.linalg.norm(a)>1e-3 else np.array([1,0,0]); b=np.cross(t,a)
        for j in range(n+1):
            th=2*math.pi*j/n; q=p+r*(math.cos(th)*a+math.sin(th)*b); V.append(tuple(q)); UV.append((j/n,i*0.2))
    for i in range(len(path)-1):
        for j in range(n):
            a=k0+i*(n+1)+j; b=a+n+1; T+=[a,b,a+1,a+1,b,b+1]
exec(open('/workspace/v3/piranha.py').read())
exec(open('/workspace/v3/stores.py').read())
exec(open('/workspace/v3/buildings.py').read())
# danger signs: posts + panels (panel mesh with UV 0..1)
signs=[(421.8,-10.5,-0.4),(330.0,4.0,0.6),(301.0,-21.0,1.0)]
PV=[];PT=[];PUV=[]; SV=[];ST=[];SUV=[]
for x,z,yaw in signs:
    g=H(x,z); box(PV,PT,PUV,(x,g-0.3,z),(0.07,2.6,0.07))
    ca,sa=math.cos(yaw),math.sin(yaw); hw=0.45
    a=(x-sa*hw+ca*0.05,g+1.55,z+ca*hw+sa*0.05); 
    pts=[(x+ca*0.05-(-sa)*0+(-sa)*s, g+yy, z+sa*0.05+ca*s) for s,yy in [(-hw,1.55),(-hw,2.35),(hw,2.35),(hw,1.55)]]
    k=len(SV); SV+=pts; SUV+=[(0,0),(0,1),(1,1),(1,0)]; ST+=[k,k+1,k+2,k,k+2,k+3]
    # back side
    back=[(p[0]+ca*0.02,p[1],p[2]+sa*0.02) for p in pts]; k=len(PV); PV+=back; PUV+=[(0,0)]*4; PT+=[k,k+2,k+1,k,k+3,k+2]
addmesh('DangerPosts','metal',PV,PT,PUV); addmesh('DangerSigns','sign_danger',SV,ST,[(1-u,v) for (u,v) in SUV])
json.dump({'items':meshes,'hide':HIDE,'hideLen':HIDEL},open('/workspace/valvert/clean/woods_meshes.json','w'))
json.dump({'pond':pond,'fanC':C},open('gen/basin.json','w'))
print('meshes',[(m['name'],len(m['v'])//3) for m in meshes])
