# East extension (x 532..964) + parking asphalt + outer ring. Outputs to /workspace/valvert/clean
import json,math,numpy as np
from PIL import Image
from scipy.ndimage import map_coordinates
class Path:
    def __init__(s,p): s.p=np.asarray(p,float)
    def contains_point(s,q,radius=0):
        x,z=q; p=s.p; ins=False; j=len(p)-1
        for i in range(len(p)):
            if (p[i][1]>z)!=(p[j][1]>z) and x<(p[j][0]-p[i][0])*(z-p[i][1])/(p[j][1]-p[i][1])+p[i][0]: ins=not ins
            j=i
        if ins or radius==0: return ins
        a=p; b=np.roll(p,-1,0); d=b-a; t=np.clip(((x-a[:,0])*d[:,0]+(z-a[:,1])*d[:,1])/((d**2).sum(1)+1e-9),0,1)
        return bool(np.min(np.hypot(a[:,0]+t*d[:,0]-x,a[:,1]+t*d[:,1]-z))<abs(radius))
OUT='/workspace/valvert/clean/'; GEN='/workspace/ValVertGame/Assets/ValVert/Gen/'
LON0,LAT0=6.107796675787339,45.88915585177747
def W(n): return ((n['lon']-LON0)*77484.85,(n['lat']-LAT0)*111132.9)
ALT=lambda a:0.99273679*a-446.608
hm=np.fromfile(OUT+'heightmap_v3.raw',dtype='<u2').reshape(2049,2049)/65535*80.056+0.002
mnt=np.asarray(Image.open('east_MNT.tif'),np.float32)  # 888x1418, x 520..964, row0 north
R=1025; X0,X1,ZS=532.0,964.0,354.646
xs=np.linspace(X0,X1,R); zs=np.linspace(-ZS,ZS,R)
gx,gz=np.meshgrid(xs,zs)
ey=ALT(map_coordinates(mnt,[(ZS-gz)*2-0.5,(gx-520)*2-0.5],order=1,mode='nearest'))
edge=np.interp(zs,np.linspace(-ZS,ZS,2049),hm[:,2048])
w=np.clip((gx-X0)/40,0,1); w=w*w*(3-2*w)
ey=(1-w)*edge[:,None]+w*ey
np.clip((ey-0.002)/80.056*65535,0,65535).astype('<u2').tofile(OUT+'east_height.raw')
def H(x,z):
    if x<=531.9:
        c=np.clip((x+532)/1064*2048,0,2048); r=np.clip((z+ZS)/709.293*2048,0,2048)
        return float(map_coordinates(hm,[[r],[c]],order=1)[0])
    c=np.clip((x-X0)/432*1024,0,1024); r=np.clip((z+ZS)/709.293*1024,0,1024)
    return float(map_coordinates(ey,[[r],[c]],order=1)[0])
# ortho crop 0.5 m
o=Image.open('full_ORTHOIMAGERY.ORTHOPHOTOS2006-2010.jpg'); print('ortho',o.size)
sx=o.size[0]/1496.0
o.crop((int(1064*sx),0,o.size[0],o.size[1])).save(GEN+'east_ortho.jpg',quality=90)
d=json.load(open('big.json'))['elements']; N={e['id']:e for e in d if e['type']=='node'}
ways=[e for e in d if e['type']=='way']
items=[]
def add(name,mat,V,T,UV):
    if T: items.append({'name':name,'mat':mat,'v':[round(float(a),3) for p in V for a in p],'t':T,'uv':[round(float(a),4) for p in UV for a in p]})
# ---- parking asphalt (draped grid) ----
PV,PT,PUV=[],[],[]
for e in ways:
    t=e.get('tags',{})
    if t.get('amenity')!='parking' or t.get('parking','surface')!='surface': continue
    pts=np.array([W(N[i]) for i in e['nodes'] if i in N])
    if len(pts)<3 or not(380<pts[:,0].mean()<964 and -200<pts[:,1].mean()<300): continue
    pa=Path(pts); x0,z0=pts.min(0); x1,z1=pts.max(0); s=1.25
    # dominant edge direction for stall lines
    L=[(np.linalg.norm(pts[i+1]-pts[i]),pts[i+1]-pts[i]) for i in range(len(pts)-1)]; dv=max(L,key=lambda a:a[0])[1]; ang=math.atan2(dv[1],dv[0]); ca,sa=math.cos(ang),math.sin(ang)
    nx=int((x1-x0)/s)+2; nz=int((z1-z0)/s)+2; idx={}
    for i in range(nx):
        for j in range(nz):
            x=x0+i*s; z=z0+j*s
    inside=lambda x,z: pa.contains_point((x,z))
    for i in range(nx-1):
        for j in range(nz-1):
            cx,cz=x0+(i+.5)*s,z0+(j+.5)*s
            if not inside(cx,cz): continue
            q=[]
            for a,b in ((i,j),(i,j+1),(i+1,j+1),(i+1,j)):
                if (a,b) not in idx:
                    x=x0+a*s; z=z0+b*s; idx[(a,b)]=len(PV); PV.append((x,H(x,z)+0.06,z))
                    u=x*ca+z*sa; v=-x*sa+z*ca; PUV.append((u/2.5,v/5.0))
                q.append(idx[(a,b)])
            PT+= [q[0],q[1],q[2],q[0],q[2],q[3]]
    print('parking',e['id'],len(pts))
add('Parking','parking',PV,PT,PUV)
# ---- east buildings ----
mnh=np.asarray(Image.open('mnh.tif'),np.float32)
def earclip(p):
    p=list(range(len(p))); P=EP; T=[]
    area=sum(P[p[i]][0]*P[p[(i+1)%len(p)]][1]-P[p[(i+1)%len(p)]][0]*P[p[i]][1] for i in range(len(p)))
    if area<0: p=p[::-1]
    g=0
    while len(p)>3 and g<5000:
        g+=1; n=len(p); ok=False
        for i in range(n):
            a,b,c=P[p[i-1]],P[p[i]],P[p[(i+1)%n]]
            if (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])<=1e-9: continue
            tri=Path([a,b,c])
            if any(tri.contains_point(P[k]) for k in p if k not in(p[i-1],p[i],p[(i+1)%n])): continue
            T+=[p[i-1],p[i],p[(i+1)%n]]; p.pop(i); ok=True; break
        if not ok: break
    if len(p)==3: T+=p
    return T
BV,BT,BUV=[],[],[]; RV,RT,RUV=[],[],[]; FOOT=[]
for e in ways:
    t=e.get('tags',{})
    if 'building' not in t or e['nodes'][0]!=e['nodes'][-1]: continue
    pts=np.array([W(N[i]) for i in e['nodes'][:-1] if i in N])
    if len(pts)<3: continue
    c=pts.mean(0)
    if not(532<c[0]<962 and -352<c[1]<352): continue
    if e['id']==62590936: continue   # Lidl is built by stores.py
    FOOT.append(pts)
    ss=[mnh[int(np.clip(ZS-z,0,708)),int(np.clip(x+532,0,1495))] for x,z in list(pts)+[tuple(c)]]
    ht=float(np.clip(np.median(ss),3,24)) if np.median(ss)>2 else (3.0*float(t.get('building:levels',2))+0.5)
    g=min(H(x,z) for x,z in pts)-0.3; top=g+0.3+ht
    for i in range(len(pts)):
        a=pts[i]; b=pts[(i+1)%len(pts)]; k=len(BV); L=float(np.linalg.norm(b-a))
        BV+=[(a[0],g,a[1]),(a[0],top,a[1]),(b[0],top,b[1]),(b[0],g,b[1])]; BUV+=[(0,g/3),(0,top/3),(L/3,top/3),(L/3,g/3)]
        BT+=[k,k+1,k+2,k,k+2,k+3]
    EP=[tuple(p) for p in pts]; tt=earclip(EP); k=len(RV)
    RV+=[(x,top,z) for x,z in pts]; RUV+=[(x/4,z/4) for x,z in pts]
    for i in range(0,len(tt),3): RT+=[k+tt[i],k+tt[i+2],k+tt[i+1]]
add('EastBuildings','store_wall',BV,BT,BUV); add('EastRoofs','roof_flat',RV,RT,RUV)
print('east buildings',len(FOOT))
# ---- east roads ----
WID={'primary':9,'secondary':8,'tertiary':7,'residential':6,'unclassified':5.5,'service':4,'living_street':5}
AV,AT,AUV=[],[],[]
for e in ways:
    hw=e.get('tags',{}).get('highway')
    if hw not in WID: continue
    pts=[np.array(W(N[i])) for i in e['nodes'] if i in N]
    if not any(p[0]>531 for p in pts): continue
    # densify
    dp=[]
    for a,b in zip(pts[:-1],pts[1:]):
        n=max(1,int(np.linalg.norm(b-a)/3))
        dp+=[a+(b-a)*k/n for k in range(n)]
    dp.append(pts[-1]); w2=WID[hw]/2; s=0
    for i,p in enumerate(dp):
        if p[0]<531 or p[0]>963.5 or abs(p[1])>354: continue
        d=dp[min(i+1,len(dp)-1)]-dp[max(i-1,0)]; d=d/(np.linalg.norm(d)+1e-9); n=np.array([-d[1],d[0]])
        if i>0: s+=float(np.linalg.norm(p-dp[i-1]))
        k=len(AV)
        for sg in (-1,1): q=p+n*w2*sg; AV.append((q[0],H(*q)+0.05+0.01*(hw!='service'),q[1])); AUV.append(((sg+1)/2*w2/2,s/4))
        if i>0 and dp[i-1][0]>=531 and dp[i-1][0]<=963.5 and abs(dp[i-1][1])<=354: AT+=[k-2,k,k+1,k-2,k+1,k-1]
add('EastRoads','asphalt',AV,AT,AUV)
# ---- east trees ----
vg=json.load(open(OUT+'veg3_flat.json')); tr=vg['trees']; ET=[]
paths=[Path(p) for p in FOOT]; RP=np.array([(v[0],v[2]) for v in AV]) if AV else np.zeros((0,2))
for i in range(0,len(tr),4):
    x,z,h=tr[i],tr[i+1],tr[i+2]
    if not(533<x<963 and -353<z<353): continue
    if any(abs(p.p[:,0].mean()-x)<80 and abs(p.p[:,1].mean()-z)<80 and p.contains_point((x,z),radius=3) for p in paths): continue
    if len(RP) and np.min(np.hypot(RP[:,0]-x,RP[:,1]-z))<3.5: continue
    ET+= [round(float(x),2),round(float(z),2),round(float(h),2)]
print('east trees',len(ET)//3)
# ---- outer ring mesh (10 m-ish DEM, ortho 2010) ----
rd=np.asarray(Image.open('ring_dem.tif'),np.float32); nr,nc=rd.shape
from scipy.ndimage import gaussian_filter as _gf; rd=_gf(rd,1.2)   # soften DSM lumps on the far ring
RX0,RX1,RZ0,RZ1=-2032,2464,-1855,1855
GV,GT,GUV=[],[],[]; idx=-np.ones((nr,nc),int)
def ins(x,z,m=0): return -532+m<x<964-m and -ZS+m<z<ZS-m
for r in range(nr):
    for c in range(nc):
        x=RX0+(c+.5)/nc*(RX1-RX0); z=RZ1-(r+.5)/nr*(RZ1-RZ0)
        if ins(x,z,20): continue
        y=ALT(rd[r,c])
        if ins(x,z): y=H(np.clip(x,-532,963.9),np.clip(z,-354,354))-0.4
        idx[r,c]=len(GV); GV.append((x,y,z)); GUV.append(((x-RX0)/(RX1-RX0),(z-RZ0)/(RZ1-RZ0)))
for r in range(nr-1):
    for c in range(nc-1):
        a,b,cc,dd=idx[r,c],idx[r,c+1],idx[r+1,c+1],idx[r+1,c]
        if min(a,b,cc,dd)<0: continue
        GT+=[int(a),int(b),int(cc),int(a),int(cc),int(dd)]
add('OuterRing','ring',GV,GT,GUV)
json.dump({'items':items,'trees':ET},open(OUT+'east_meshes.json','w'))
print({i['name']:len(i['t'])//3 for i in items})
