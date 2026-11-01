# ---- Piranha basin v4: dam (mouth), stone rim, puddles, tail fin, bollards, fences, stone house ----
import random
rnd=random.Random(7)
HIDE=[];HIDEL=[]
def mk(): return [],[],[]
def wallseg(V,T,UV,p0,p1,y0a,y1a,y0b,y1b,th=0.5,s=1.2):
    # thick wall between plan points p0,p1 ; bottoms y0a/y0b tops y1a/y1b
    p0=np.array(p0,float);p1=np.array(p1,float); d=p1-p0; u=d/np.linalg.norm(d); n=np.array([-u[1],u[0]])*th/2
    A0=p0+n;A1=p0-n;B0=p1+n;B1=p1-n
    P3=lambda q,y:(q[0],y,q[1])
    quad(V,T,UV,P3(A0,y0a),P3(A0,y1a),P3(B0,y1b),P3(B0,y0b),s)
    quad(V,T,UV,P3(B1,y0b),P3(B1,y1b),P3(A1,y1a),P3(A1,y0a),s)
    quad(V,T,UV,P3(A0,y1a),P3(A1,y1a),P3(B1,y1b),P3(B0,y1b),s)
    quad(V,T,UV,P3(A1,y0a),P3(A1,y1a),P3(A0,y1a),P3(A0,y0a),s)
    quad(V,T,UV,P3(B0,y0b),P3(B0,y1b),P3(B1,y1b),P3(B1,y0b),s)
# ---------- DAM ----------
fl=5.35; TOP=10.95; XF=424.6; ZC=-3.0; AW=1.45; SPR=8.55
CV,CT,CUV=mk()   # board-formed concrete
DV,DT,DUV=mk()   # dark
SV_,ST_,SUV_=mk()# stone
MV,MT,MUV=mk()   # grey metal
# front face with arch opening (normal -x)
def fq(z0,z1,y0,y1,x=XF): quad(CV,CT,CUV,(x,y0,z1),(x,y1,z1),(x,y1,z0),(x,y0,z0),1.2)  # z1>z0, faces -x
fq(ZC+AW,-0.5,fl-0.3,TOP); fq(-6.2,ZC-AW,fl-0.3,TOP)
arc=[(ZC+AW*math.cos(t),SPR+AW*math.sin(t)) for t in np.linspace(0,math.pi,17)]
for (za,ya),(zb,yb) in zip(arc[:-1],arc[1:]):  # za>zb
    quad(CV,CT,CUV,(XF,ya,za),(XF,TOP,za),(XF,TOP,zb),(XF,yb,zb),1.2)
# arch reveal (tunnel 5 m deep) concrete + dark back
X2=XF+5.0
outline=[(ZC+AW,fl-0.3)]+arc+[(ZC-AW,fl-0.3)]
for (za,ya),(zb,yb) in zip(outline[:-1],outline[1:]):
    quad(CV,CT,CUV,(XF,ya,za),(XF,yb,zb),(X2,yb,zb),(X2,ya,za),1.2)
quad(DV,DT,DUV,(X2,fl-0.3,ZC-AW),(X2,TOP,ZC-AW),(X2,TOP,ZC+AW),(X2,fl-0.3,ZC+AW))
quad(DV,DT,DUV,(XF+0.3,fl+0.02,ZC+AW),(X2,fl+0.02,ZC+AW),(X2,fl+0.02,ZC-AW),(XF+0.3,fl+0.02,ZC-AW))
# slight proud frame (pilasters) either side of arch
box(CV,CT,CUV,(XF-0.15,fl-0.3,ZC+AW+0.35),(0.3,TOP-fl+0.3,0.7),0,1.2)
box(CV,CT,CUV,(XF-0.15,fl-0.3,ZC-AW-0.35),(0.3,TOP-fl+0.3,0.7),0,1.2)
# north lower wall whose top slopes down (stair side)
ZN=8.6; YN=6.1
quad(CV,CT,CUV,(XF,fl-0.3,ZN),(XF,YN,ZN),(XF,TOP,-0.5),(XF,fl-0.3,-0.5),1.2)
quad(CV,CT,CUV,(XF,YN,ZN),(XF+1.2,YN,ZN),(XF+1.2,TOP,-0.5),(XF,TOP,-0.5),1.2)
quad(CV,CT,CUV,(XF,fl-0.3,ZN),(XF+1.2,fl-0.3,ZN),(XF+1.2,YN,ZN),(XF,YN,ZN),1.2)
# stair climbing along the face (from north/low to south/high)
nst=27
for i in range(nst):
    z=ZN-0.2-i*(ZN-0.2-(-0.2))/nst; y=fl+(i+1)*(TOP-fl)/nst
    box(CV,CT,CUV,(XF-0.65,fl-0.3,z),(1.2,y-fl+0.3,(ZN+0.0)/nst+0.02),0,1.0)
# stair handrail (grey metal) on outer edge
rail=[(XF-1.2,fl+(i)*(TOP-fl)/nst+1.0,ZN-0.2-i*(ZN)/nst) for i in range(nst+1)]
tube(MV,MT,MUV,rail,0.035)
for i in range(0,nst+1,5): r=rail[i]; box(MV,MT,MUV,(r[0],r[1]-1.0,r[2]),(0.05,1.0,0.05))
# south: stone masonry retaining wall, slightly battered
quad(SV_,ST_,SUV_,(XF+0.0,fl-0.3,-6.2),(XF+0.5,TOP-0.2,-6.2),(XF+0.6,TOP-0.6,-14.5),(XF+0.1,fl-0.3,-14.5),2.0)
# top slab + coping + fill block
box(CV,CT,CUV,((XF+431)/2,TOP-0.35,(-14.5-0.5)/2),(431-XF,0.6,14.0),0,1.5)
box(CV,CT,CUV,(XF+0.3,TOP,-7.5),(0.8,0.3,13.4),0,1.2)
# green railing on dam top
GV,GT,GUV=mk()
FV,FT,FUV=mk()
def fence(path,h=1.1,post=2.0):
    path=[np.array(p,float) for p in path]
    for a,b in zip(path[:-1],path[1:]):
        L=np.linalg.norm(b-a); n=max(1,int(L/post))
        for k in range(n+1):
            q=a+(b-a)*k/n; g=H(*q) if q[0]<XF-0.5 or q[0]>431 else TOP+0.3
            box(GV,GT,GUV,(q[0],g-0.2,q[1]),(0.06,h+0.2,0.06))
        ga=H(*a) if a[0]<XF-0.5 or a[0]>431 else TOP+0.3; gb=H(*b) if b[0]<XF-0.5 or b[0]>431 else TOP+0.3
        k=len(FV); FV.extend([(a[0],ga+0.05,a[1]),(a[0],ga+h,a[1]),(b[0],gb+h,b[1]),(b[0],gb+0.05,b[1])])
        FUV.extend([(0,0),(0,h),(L,h),(L,0)]); FT.extend([k,k+1,k+2,k,k+2,k+3])
        tube(GV,GT,GUV,[(a[0],ga+h,a[1]),(b[0],gb+h,b[1])],0.03,6)
fence([(XF+0.1,-14.3),(XF+0.1,-0.6)])
# ---------- stone rim around the basin (white edge on the 2010 ortho) ----------
pp=np.array(pond+[pond[0]],float); pts=dens(pp,1.15)
for q in pts[:-1]:
    if q[0]>XF-2.5 and -15<q[1]<9: continue
    g=H(*q); sx=0.8+rnd.random()*0.6
    box(SV_,ST_,SUV_,(q[0]+rnd.uniform(-.15,.15),g-0.45,q[1]+rnd.uniform(-.15,.15)),(sx,0.75+rnd.random()*0.45,0.7+rnd.random()*0.4),rnd.random()*3.14,1.0)
for z_,s_ in ((9.35,1),(-15.1,-1)):
    xs_=np.linspace(410,XF,8)
    for xa,xb in zip(xs_[:-1],xs_[1:]):
        ha=max(H(xa,z_+0.9*s_),H(xa,z_))+0.25; hb=max(H(xb,z_+0.9*s_),H(xb,z_))+0.25
        wallseg(SV_,ST_,SUV_,(xa,z_),(xb,z_),fl-0.4,ha,fl-0.4,hb,0.7,2.0)
# big boulders retaining wall left of the dam (p13 foreground/left)
for t in np.linspace(0,1,16):
    q=np.array([XF-1.5,9.5])+(np.array([414,13.5])-np.array([XF-1.5,9.5]))*t; g=H(*q)
    for lay in range(2): box(SV_,ST_,SUV_,(q[0],g-0.6+lay*0.8,q[1]),(1.3,0.95,1.0),rnd.random()*3.14,1.0)

# ---------- FISH HEAD sculpted on the dam face (mouth = arch) ----------
def disc(V,T,UV,cz,cy,r,x,n=20):
    k=len(V); V.append((x,cy,cz)); UV.append((0.5,0.5))
    for i in range(n+1):
        a=2*math.pi*i/n; V.append((x,cy+r*math.sin(a),cz+r*math.cos(a))); UV.append((0.5+0.5*math.cos(a),0.5+0.5*math.sin(a)))
    for i in range(n): T.extend([k,k+1+i,k+2+i])
def ring3(V,T,UV,cz,cy,r0,r1,x0,x1,n=24):
    for i in range(n):
        a=2*math.pi*i/n; b=2*math.pi*(i+1)/n
        p=lambda r,t,x:(x,cy+r*math.sin(t),cz+r*math.cos(t))
        quad(V,T,UV,p(r1,a,x1),p(r1,b,x1),p(r0,b,x0),p(r0,a,x0),0.8)
        quad(V,T,UV,p(r1,b,x1),p(r1,a,x1),p(r1,a,x0+0.0),p(r1,b,x0+0.0),0.8)
for sz in (-1,1):
    ez=ZC+sz*2.05; ey=SPR+1.25
    ring3(CV,CT,CUV,ez,ey,0.0,0.62,XF-0.32,XF-0.02)   # domed eye socket
    disc(DV,DT,DUV,ez,ey,0.30,XF-0.335)                 # pupil
    disc(CV,CT,CUV,ez+sz*0.08,ey+0.1,0.07,XF-0.34,8)    # glint bump
    # gill arcs: three curved ridges behind each eye
    for g,gz in enumerate((2.55,2.75,2.95)):
        pts=[(ZC+sz*(gz-0.35*math.cos(t)),6.0+t/math.pi*4.3) for t in np.linspace(0.15,math.pi-0.15,12)]
        for (z0,y0),(z1,y1) in zip(pts[:-1],pts[1:]):
            quad(CV,CT,CUV,(XF-0.12,y0,z0-0.06),(XF-0.12,y1,z1-0.06),(XF-0.12,y1,z1+0.06),(XF-0.12,y0,z0+0.06),0.5)
            quad(CV,CT,CUV,(XF,y0,z0-0.06),(XF-0.12,y0,z0-0.06),(XF-0.12,y1,z1-0.06),(XF,y1,z1-0.06),0.5)
            quad(CV,CT,CUV,(XF-0.12,y0,z0+0.06),(XF,y0,z0+0.06),(XF,y1,z1+0.06),(XF-0.12,y1,z1+0.06),0.5)
# teeth: triangular concrete teeth hanging from the arch edge (upper jaw) and rising from the sill (lower jaw)
TWV,TWT,TWUV=mk()
for t in np.linspace(0.12,math.pi-0.12,9):
    z=ZC+AW*math.cos(t); y=SPR+AW*math.sin(t); dz=-math.cos(t); dy=-math.sin(t); w=0.17
    tz,ty=z+dz*0.45,y+dy*0.45; ox=XF-0.08
    pz,py=-dy*w,dz*w
    tri(TWV,TWT,TWUV,(ox,y+py,z+pz),(ox,ty,tz),(ox,y-py,z-pz)); tri(TWV,TWT,TWUV,(ox,y-py,z-pz),(ox,ty,tz),(ox,y+py,z+pz))
for z in np.linspace(ZC-AW+0.25,ZC+AW-0.25,6):
    y=fl+0.45; tri(TWV,TWT,TWUV,(XF-0.25,y,z-0.15),(XF-0.25,y+0.42,z),(XF-0.25,y,z+0.15)); tri(TWV,TWT,TWUV,(XF-0.25,y,z+0.15),(XF-0.25,y+0.42,z),(XF-0.25,y,z-0.15))
addmesh('FishTeeth','white_trim',TWV,TWT,TWUV)
# lower jaw: protruding lip slab under the mouth + chin
box(CV,CT,CUV,(XF-0.45,fl-0.3,ZC),(0.9,0.75,2*AW+1.4),0,1.2)
box(CV,CT,CUV,(XF-0.25,fl-0.3,ZC),(0.5,0.3,2*AW+2.2),0,1.2)
# brow ridges over eyes and snout ridge above the mouth
for sz in (-1,1): box(CV,CT,CUV,(XF-0.2,SPR+2.0,ZC+sz*2.05),(0.4,0.22,1.4),0,1.0)
box(CV,CT,CUV,(XF-0.12,SPR+AW+0.25,ZC),(0.24,0.2,2*AW+0.4),0,1.0)
addmesh('DamConcrete','concrete_board',CV,CT,CUV); addmesh('DamDark','dark',DV,DT,DUV); addmesh('StoneWalls','stone',SV_,ST_,SUV_)
# ---------- murky puddles on the apron ----------
WV,WT,WUV=mk(); idx={}
xs=np.arange(345,425,0.6); zs=np.arange(-30,12,0.6)
inP=lambda x,z: P[int(round((z+354.646)/SZ)),int(round((x+532)/SX))]
noise=lambda x,z: math.sin(x*0.21+1.3)*math.cos(z*0.27)+0.6*math.sin(x*0.07+z*0.11+2)+0.4*math.cos(x*0.5-z*0.35)
for i,x in enumerate(xs):
    for j,z in enumerate(zs):
        if inP(x,z) and noise(x,z)>0.55: idx[(i,j)]=len(WV); WV.append((x,H(x,z)+0.035,z)); WUV.append((x/4,z/4))
for (i,j),k in idx.items():
    if (i+1,j) in idx and (i,j+1) in idx and (i+1,j+1) in idx: a_,b_,c_,d_=k,idx[(i+1,j)],idx[(i,j+1)],idx[(i+1,j+1)]; WT+=[a_,c_,b_,b_,c_,d_]
addmesh('Puddles','water',WV,WT,WUV)
# ---------- TAIL: big triangular fin with curved green tube ----------
TV,TT,TUV=mk(); RV,RT,RUV=mk()
_u=np.array([27.1,7.5]);_u/=np.linalg.norm(_u)
_u=np.array([23.5,13.8]);_u/=np.linalg.norm(_u); A=np.array([431.0,2.5]); B=A+_u*12.0; nseg=8   # tail grows out of the back of the dam (head) along steps 260433024
YA=H(*A)+5.5; YB=H(*B)+0.25
tops=[]
for k in range(nseg):
    t0,t1=k/nseg,(k+1)/nseg; p0=A+(B-A)*t0; p1=A+(B-A)*t1
    g0=H(*p0); g1=H(*p1); gA=H(*A); gB=H(*B)
    y0=YA+(YB-YA)*t0; y1=YA+(YB-YA)*t1
    wallseg(TV,TT,TUV,p0,p1,min(g0,g1,TOP-1)-0.6,y0,min(g0,g1,TOP-1)-0.6,y1,0.55,1.4); tops.append((p0,y0))
tops.append((B,YB))
u=(B-A)/np.linalg.norm(B-A); nn=np.array([-u[1],u[0]])
if nn[1]>0: nn=-nn
rp=[]
for s in np.linspace(0,1,24):     # tube rides slightly outboard of the sloped edge
    q=A+(B-A)*s; gA=H(*A); gB=H(*B); y=YA+(YB-YA)*s+0.18; rp.append((q[0]+nn[0]*0.35,y,q[1]+nn[1]*0.35))
for s in np.linspace(0.1,1,10):   # top: curls up/back over the tall end
    a=s*math.pi*0.55; q=A-u*0.9*math.sin(a); rp.insert(0,(q[0]+nn[0]*0.35,YA+0.18+0.9*(1-math.cos(a)),q[1]+nn[1]*0.35))
for s in np.linspace(0.1,1,8):    # bottom: sweeps down and becomes handrail along the steps
    a=s*math.pi/2; q=B+u*1.2*math.sin(a); rp.append((q[0]+nn[0]*0.35,YB+0.18-0.5*math.sin(a),q[1]+nn[1]*0.35))
tube(RV,RT,RUV,rp,0.12,10)
addmesh('TailFin','concrete_board',TV,TT,TUV)
# ---------- two concrete stair flights (OSM 260433023 handrail=yes, 260433024), fin between them ----------
STV,STT,STUV=mk()
def stairs(lo,hi,w=1.5,rise=0.17):
    lo=np.array(lo);hi=np.array(hi); d=hi-lo; L=np.linalg.norm(d); d/=L; yaw=math.atan2(d[1],d[0])
    n=int(L/0.34); tr=L/n; nv=np.array([-d[1],d[0]])
    Y0=H(*lo);Y1=H(*hi); ys=[round((Y0+(Y1-Y0)*(i+0.5)/n)/rise)*rise+0.03 for i in range(n)]
    prof=lambda t: ys[min(n-1,int(t*n))]
    for i in range(n):
        p=lo+d*tr*(i+0.5); yt=ys[i]; gb=min(yt,H(*p))-0.6
        box(STV,STT,STUV,(p[0],gb,p[1]),(tr+0.02,yt-gb,w),yaw,1.0)
        for sg in (-1,1):
            q=p+nv*sg*(w/2+0.1); box(STV,STT,STUV,(q[0],gb,q[1]),(tr+0.02,yt-gb+0.2,0.2),yaw,1.0)
    return lo,d,L,prof
S23=stairs((438.8,-11.1),(458.7,9.1)); S24=stairs((427.3,9.8),(454.4,17.3))
def railalong(S,side,V,T,UV,t0=0.0,r=0.045):
    lo,d,L,prof=S; nv=np.array([-d[1],d[0]])*side
    pts=[(lo+d*L*t+nv*0.95,prof(t)) for t in np.linspace(t0,1,14)]
    tube(V,T,UV,[(q[0],y+0.95,q[1]) for q,y in pts],r,8)
    for q,y in pts[::2]: box(V,T,UV,(q[0],y-0.3,q[1]),(0.06,1.25,0.06))
    return pts
_c=(A+B)/2
def fin_side(S):  # +1/-1: which side of the flight faces the fin
    lo,d,L,_=S; nv=np.array([-d[1],d[0]]); return 1 if np.dot(_c-lo,nv)>0 else -1
railalong(S23,fin_side(S23),GV,GT,GUV)
# ornamental grasses (pampas / miscanthus-like) on the slope around the fin
OV,OT,OUV=mk()
for i in range(34):
    q=A+(B-A)*rnd.random()+nn*rnd.uniform(-5.5,-0.8)
    g=H(*q); hgt=0.7+rnd.random()*0.6; w=0.6+rnd.random()*0.4
    for a in (0,1.05,2.1):
        ca,sa=math.cos(a+i),math.sin(a+i); k=len(OV)
        OV+= [(q[0]-ca*w,g-0.05,q[1]-sa*w),(q[0]-ca*w*1.3,g+hgt,q[1]-sa*w*1.3),(q[0]+ca*w*1.3,g+hgt,q[1]+sa*w*1.3),(q[0]+ca*w,g-0.05,q[1]+sa*w)]
        OUV+=[(0,0),(0,1),(1,1),(1,0)]; OT+=[k,k+1,k+2,k,k+2,k+3]
addmesh('OrnGrass','orn_grass',OV,OT,OUV)
# ---------- green & white striped bollards along the footway by the road ----------
BV,BT,BUV=mk()
for p0,p1,n in [((456.5,15.5),(468.5,2.5),6),((452,-1),(437,-15),5)]:
    for k in range(n):
        q=np.array(p0)+(np.array(p1)-np.array(p0))*k/(n-1); g=H(*q); k0=len(BV); seg=12
        for j in range(seg+1):
            th=2*math.pi*j/seg
            for yy in (0,1.15): BV.append((q[0]+0.07*math.cos(th),g-0.1+yy,q[1]+0.07*math.sin(th))); BUV.append((j/seg,yy/1.15))
        for j in range(seg): a_=k0+2*j; BT+=[a_,a_+1,a_+2,a_+1,a_+3,a_+2]
        box(BV,BT,BUV,(q[0],g+1.05,q[1]),(0.16,0.06,0.16))
# ---------- photo 2 extras: railing continues up the stairs, boxwood balls, small conifers ----------
_s0=np.array([427.3,9.8]); _s1=np.array([454.4,17.3])
_pts=railalong(S24,fin_side(S24),RV,RT,RUV,0.4,0.06)   # green tube continues as the stair railing on the fin side
q0,y0_=_pts[0]; tube(RV,RT,RUV,[rp[-1],(q0[0],y0_+0.95,q0[1])],0.06,8)
BXV,BXT,BXUV=mk(); CNV,CNT,CNUV=mk()
def blob(V,T,UV,c,r,ry,n=8,m=6):
    k=len(V)
    for i in range(m+1):
        ph=math.pi*i/m
        for j in range(n+1):
            th=2*math.pi*j/n; f=1+0.08*math.sin(3*th+i)
            V.append((c[0]+r*f*math.sin(ph)*math.cos(th),c[1]+ry*math.cos(ph),c[2]+r*f*math.sin(ph)*math.sin(th))); UV.append((j/n,i/m))
    for i in range(m):
        for j in range(n): a=k+i*(n+1)+j; b=a+n+1; T.extend([a,a+1,b,a+1,b+1,b])
def cone(V,T,UV,c,r,h,n=10):
    k=len(V); V.append((c[0],c[1]+h,c[2])); UV.append((0.5,1))
    for j in range(n+1):
        th=2*math.pi*j/n; V.append((c[0]+r*math.cos(th),c[1]+0.15,c[2]+r*math.sin(th))); UV.append((j/n,0))
    for j in range(n): T.extend([k,k+2+j,k+1+j])
for i in range(8):
    t=rnd.random(); q=_s0+(_s1-_s0)*(0.15+0.85*t)+nn*rnd.uniform(1.6,4.5)
    r=rnd.uniform(0.25,0.4); blob(BXV,BXT,BXUV,(q[0],H(*q)+r*0.8,q[1]),r,r*0.85)
for q in [(446.5,11.0)]:
    hh=rnd.uniform(1.4,2.4); cone(CNV,CNT,CNUV,(q[0],H(*q)-0.1,q[1]),hh*0.32,hh)
addmesh('Stairs','concrete_wall',STV,STT,STUV); addmesh('Boxwood','boxwood',BXV,BXT,BXUV); addmesh('Conifers','conifer',CNV,CNT,CNUV)
addmesh('Bollards','bollard',BV,BT,BUV)
addmesh('GreenFencePosts','green_metal',GV,GT,GUV); addmesh('GreenFenceMesh','fence_green',FV,FT,FUV)
# ---------- old stone house across the avenue (OSM way 62594636), seen behind the dam ----------
osm=json.load(open('/workspace/v3/big.json'))['elements']; ND={e['id']:e for e in osm if e['type']=='node'}
way=[e for e in osm if e['type']=='way' and e['id']==62594636][0]
fp=[((ND[n]['lon']-6.107796675787339)*77484.85,(ND[n]['lat']-45.88915585177747)*111132.9) for n in way['nodes']]
if fp[0]==fp[-1]: fp=fp[:-1]
HIDE+= [c for q in fp for c in q]; HIDEL.append(len(fp))
# use oriented bounding box of the footprint -> simple hip-roof house
fa=np.array(fp); cen=fa.mean(0); cov=np.cov((fa-cen).T); ev,evec=np.linalg.eigh(cov); ax=evec[:,1]; ay=np.array([-ax[1],ax[0]])
lx=(fa-cen)@ax; ly=(fa-cen)@ay; cen=cen+ax*(lx.max()+lx.min())/2+ay*(ly.max()+ly.min())/2
hx=(lx.max()-lx.min())/2*0.92; hy=(ly.max()-ly.min())/2*0.92
g=min(H(*q) for q in fp); eave=g+6.6; ridge=eave+3.4
P2=lambda a,b:cen+ax*a+ay*b
HV,HT,HUV=mk(); RFV,RFT,RFUV=mk(); WiV,WiT,WiUV=mk(); TrV,TrT,TrUV=mk()
cs=[P2(-hx,-hy),P2(hx,-hy),P2(hx,hy),P2(-hx,hy)]
for i in range(4):
    a_=cs[i];b_=cs[(i+1)%4]
    quad(HV,HT,HUV,(b_[0],g-0.5,b_[1]),(b_[0],eave,b_[1]),(a_[0],eave,a_[1]),(a_[0],g-0.5,a_[1]),2.0)
    # windows: 2 storeys
    d=b_-a_; L=np.linalg.norm(d); u2=d/L; on=np.array([u2[1],-u2[0]])*-0.03
    nwin=max(1,int(L/3.2))
    for k in range(nwin):
        c=a_+d*(k+0.5)/nwin+on
        for y0 in (g+1.0,g+4.0):
            w=0.55; c0=c-u2*w; c1=c+u2*w
            quad(WiV,WiT,WiUV,(c1[0],y0,c1[1]),(c1[0],y0+1.5,c1[1]),(c0[0],y0+1.5,c0[1]),(c0[0],y0,c0[1]))
            e0=c-u2*(w+0.15)+on; e1=c+u2*(w+0.15)+on   # light stone surround
            quad(TrV,TrT,TrUV,(e1[0],y0-0.15,e1[1]),(e1[0],y0,e1[1]),(e0[0],y0,e0[1]),(e0[0],y0-0.15,e0[1]))
            quad(TrV,TrT,TrUV,(e1[0],y0+1.5,e1[1]),(e1[0],y0+1.68,e1[1]),(e0[0],y0+1.68,e0[1]),(e0[0],y0+1.5,e0[1]))
# hip roof with overhang
o=0.6; rc=[P2(-hx-o,-hy-o),P2(hx+o,-hy-o),P2(hx+o,hy+o),P2(-hx-o,hy+o)]; ry=min(hx,hy)
R3=lambda q,y:(q[0],y,q[1])
if hx>=hy: r0=P2(-hx+ry*0.9,0); r1=P2(hx-ry*0.9,0); E_=[(0,r0,r1),(1,r1,r1),(2,r1,r0),(3,r0,r0)]
else: r0=P2(0,-hy+hx*0.9); r1=P2(0,hy-hx*0.9); E_=[(0,r0,r0),(1,r0,r1),(2,r1,r1),(3,r1,r0)]
for i,ra,rb in E_:
    a_=rc[i]; b_=rc[(i+1)%4]
    if ra is rb: tri(RFV,RFT,RFUV,R3(a_,eave-0.3),R3(ra,ridge),R3(b_,eave-0.3),2.0)
    else: quad(RFV,RFT,RFUV,R3(a_,eave-0.3),R3(ra,ridge),R3(rb,ridge),R3(b_,eave-0.3),2.0)
# front gable dormer facing the dam (west-ish) + chimneys
for t in (-0.55,0.1,0.6):
    q=P2(hx*t,hy*0.25*(1 if t>0 else -1)); box(HV,HT,HUV,(q[0],ridge-1.6,q[1]),(0.6,2.6,0.6),0,1.0)
addmesh('StoneHouse','stone_house',HV,HT,HUV); addmesh('StoneHouseRoof','roof_tiles',RFV,RFT,RFUV)
addmesh('StoneHouseWin','window',WiV,WiT,WiUV); addmesh('StoneHouseTrim','white_trim',TrV,TrT,TrUV)
print('house',cen,hx,hy,g)

# ---------- the 'piranha': continuous green steel tube tracing the fish contour (head/pond, body/lawn, tail) ----------
addmesh('TailTube','green_metal',RV,RT,RUV)  # contour loop on hold
