# ---- 2010 stores, school, bus shelter, parking lamps (OSM ODbL footprints, self-made signs) ----
osm=json.load(open('/workspace/v3/big.json'))['elements']; ND={e['id']:e for e in osm if e['type']=='node'}
WY={e['id']:e for e in osm if e['type']=='way'}
W2=lambda n:((ND[n]['lon']-6.107796675787339)*77484.85,(ND[n]['lat']-45.88915585177747)*111132.9)
def ring(wid):
    r=[W2(n) for n in WY[wid]['nodes'] if n in ND]
    if r[0]==r[-1]: r=r[:-1]
    a=sum(r[i][0]*r[(i+1)%len(r)][1]-r[(i+1)%len(r)][0]*r[i][1] for i in range(len(r)))
    return r if a>0 else r[::-1]   # CCW (x,z)
_EH=np.fromfile('/workspace/valvert/clean/east_height.raw',dtype='<u2').reshape(1025,1025)/65535*80.056+0.002
def Hc(x,z):
    z=min(max(z,-354),354)
    if x<=531: return H(max(x,-531),z)
    c=min((x-532)/432*1024,1024); r=(z+354.646)/709.293*1024; c0=int(min(max(c,0),1023)); r0=int(min(r,1023)); fc=c-c0; fr=r-r0
    return float(_EH[r0,c0]*(1-fc)*(1-fr)+_EH[r0,c0+1]*fc*(1-fr)+_EH[r0+1,c0]*(1-fc)*fr+_EH[r0+1,c0+1]*fc*fr)
def inpoly(p,q):
    ins=False; j=len(q)-1
    for i in range(len(q)):
        if (q[i][1]>p[1])!=(q[j][1]>p[1]) and p[0]<(q[j][0]-q[i][0])*(p[1]-q[i][1])/(q[j][1]-q[i][1])+q[i][0]: ins=not ins
        j=i
    return ins
def earclip(r):
    idx=list(range(len(r))); T=[]; guard=0
    cr=lambda a,b,c:(b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])
    while len(idx)>3 and guard<5000:
        guard+=1; n=len(idx)
        for k in range(n):
            i0,i1,i2=idx[k-1],idx[k],idx[(k+1)%n]
            if cr(r[i0],r[i1],r[i2])<=1e-9: continue
            if any(inpoly(r[j],[r[i0],r[i1],r[i2]]) for j in idx if j not in(i0,i1,i2)): continue
            T.append((i0,i1,i2)); idx.pop(k); break
        else: break
    if len(idx)==3: T.append(tuple(idx))
    return T
def uvquad(V,T,UV,a,b,c,d):
    k=len(V); V.extend([a,b,c,d]); UV.extend([(0,0),(0,1),(1,1),(1,0)]); T.extend([k,k+1,k+2,k,k+2,k+3])
M={}
def G(m):
    if m not in M: M[m]=([],[],[])
    return M[m]
def extrude(r,ht,wall,roof='roof_flat',hide=True):
    if hide: HIDE.extend([c for q in r for c in q]); HIDEL.append(len(r))
    g=min(Hc(*q) for q in r)-0.3; top=max(Hc(*q) for q in r)+ht
    V,T,UV=G(wall)
    for i in range(len(r)):
        a=r[i]; b=r[(i+1)%len(r)]
        quad(V,T,UV,(b[0],g,b[1]),(b[0],top,b[1]),(a[0],top,a[1]),(a[0],g,a[1]),3.5 if wall=='school_wall' else 3.0)
        if wall=='gym_wall':
            L_=float(np.hypot(b[0]-a[0],b[1]-a[1]))/6.0; UV[-4:]=[(L_,0),(L_,1),(0,1),(0,0)]
        if wall=='school_wall':
            rows=max(1,round(ht/3.5)); L_=float(np.hypot(b[0]-a[0],b[1]-a[1]))/3.5; UV[-4:]=[(L_,0),(L_,rows),(0,rows),(0,0)]
        # parapet cap
    V,T,UV=G(roof); k=len(V)
    for q in r: V.append((q[0],top,q[1])); UV.append((q[0]/4,q[1]/4))
    for a,b,c in earclip(r): T.extend([k+a,k+c,k+b])
    return g,top
def facing(r,target):
    best=None
    for i in range(len(r)):
        a=np.array(r[i]);b=np.array(r[(i+1)%len(r)]); d=b-a; L=np.linalg.norm(d); n=np.array([d[1],-d[0]])/L  # outward for CCW
        m=(a+b)/2; s=L*max(0,np.dot(n,(np.array(target)-m)/np.linalg.norm(np.array(target)-m)))
        if best is None or s>best[0]: best=(s,a,b,n,L)
    return best[1:]
def panel(mat,a,b,n,y0,y1,w,frac=0.5,off=0.06):
    d=(b-a)/np.linalg.norm(b-a); c=a+(b-a)*frac+n*off
    right=np.array([-n[1],n[0]])   # viewer facing -n
    p0=c-right*w/2; p1=c+right*w/2
    V,T,UV=G(mat); uvquad(V,T,UV,(p0[0],y0,p0[1]),(p0[0],y1,p0[1]),(p1[0],y1,p1[1]),(p1[0],y0,p1[1]))
STORES=[(62584900,'sign_grandfrais',(471,115),7.0,5.33),(62593722,'sign_aldi',(480,64),6.0,4.0),(62590936,'sign_lidl',(586,-62),7.0,1.0)]
for wid,sg,park,ht,asp in STORES:
    r=ring(wid); g,top=extrude(r,ht,'store_wall')
    a,b,n,L=facing(r,park)
    panel('shopfront',a,b,n,g+0.3,g+3.6,min(L*0.6,24),0.5,0.04)
    sh=2.2 if asp>1.5 else 3.2
    panel(sg,a,b,n,top-sh-0.3,top-0.3,sh*asp,0.5 if asp>1.5 else 0.2,0.08)
    print('store',sg,'ground',round(g,1),'edge',a.round(1),b.round(1))
# --- pharmacy (node ~ (572,118)): green-cross sign on the nearest building
_ph=np.array([572.0,118.0]); _best=None
for e in WY.values():
    if 'building' in e.get('tags',{}) and e['nodes'][0] in ND and e['nodes'][0]==e['nodes'][-1]:
        r=ring(e['id']); dd=np.linalg.norm(np.mean(r,0)-_ph)
        if _best is None or dd<_best[0]: _best=(dd,r,e['id'])
r=_best[1]; a,b,n,L=facing(r,_ph+np.array([-25.0,0])); g=min(Hc(*q) for q in r)
panel('sign_pharma',a,b,n,g+2.6,g+3.8,1.2,0.5,0.1)
print('pharmacy sign on',_best[2],'dist',round(_best[0],1))
from PIL import Image as _I
_MNH=np.asarray(_I.open('/workspace/v3/mnh.tif'),np.float32)
# --- school: hide + rebuild every OSM building inside the school polygon with a 2010 school look
sp=ring(1009134376); first=True
for e in WY.values():
    if 'building' in e.get('tags',{}) and e['nodes'][0] in ND:
        r=ring(e['id']); c=np.mean(r,0)
        if inpoly(c,sp):
            ss=[_MNH[int(np.clip(354.646-q[1],0,708)),int(np.clip(q[0]+532,0,1495))] for q in list(r)+[tuple(c)]]
            hh=float(np.median(ss)); hh=3.6 if hh<4.8 else 7.0   # 1 or 2 storeys (DSM)
            g,top=extrude(r,hh,'school_wall'); print('school bld',e['id'],hh)
            if e['id']==62596483:
                a,b,n,L=facing(r,(348,205)); panel('sign_ecole',a,b,n,top-1.2,top-0.2,min(L*0.8,9),0.5,0.08); first=False
                print('school sign on',e['id'])
# school green fence along the polygon
SFV,SFT,SFUV=G('fence_green'); SGV,SGT,SGUV=G('green_metal')
for i in range(len(sp)):
    a=np.array(sp[i]);b=np.array(sp[(i+1)%len(sp)]); L=np.linalg.norm(b-a)
    ga,gb=Hc(*a),Hc(*b); k=len(SFV)
    SFV.extend([(a[0],ga,a[1]),(a[0],ga+1.6,a[1]),(b[0],gb+1.6,b[1]),(b[0],gb,b[1])]); SFUV.extend([(0,0),(0,1.6),(L,1.6),(L,0)]); SFT.extend([k,k+1,k+2,k,k+2,k+3])
    for t in np.arange(0,L,2.5): q=a+(b-a)*t/L; box(SGV,SGT,SGUV,(q[0],Hc(*q)-0.2,q[1]),(0.06,1.8,0.06))
# --- bus shelter (2010 style: steel frame, glass, sign) on the OSM platform
pa=np.array(W2(WY[628404998]['nodes'][0])); pb=np.array(W2(WY[628404998]['nodes'][-1]))
c=(pa+pb)/2; u=(pb-pa)/np.linalg.norm(pb-pa); nrm=np.array([-u[1],u[0]])
# back wall on the side away from the avenue (avenue runs along the platform to the east)
if nrm[0]>0: nrm=-nrm
g=Hc(*c); MV2,MT2,MUV2=G('metal'); GV2,GT2,GUV2=G('glass')
L2=4.2; D2=1.5
corners=[c-u*L2/2,c+u*L2/2,c-u*L2/2+nrm*D2,c+u*L2/2+nrm*D2]
for q in corners: box(MV2,MT2,MUV2,(q[0],g-0.1,q[1]),(0.08,2.5,0.08))
k0=corners
uvquad(GV2,GT2,GUV2,(k0[2][0],g+0.15,k0[2][1]),(k0[2][0],g+2.3,k0[2][1]),(k0[3][0],g+2.3,k0[3][1]),(k0[3][0],g+0.15,k0[3][1]))
for s0,s1 in ((k0[0],k0[2]),(k0[1],k0[3])): uvquad(GV2,GT2,GUV2,(s0[0],g+0.15,s0[1]),(s0[0],g+2.3,s0[1]),(s1[0],g+2.3,s1[1]),(s1[0],g+0.15,s1[1]))
RV2,RT2,RUV2=G('roof_shelter'); uvquad(RV2,RT2,RUV2,(k0[0][0],g+2.45,k0[0][1]),(k0[2][0],g+2.5,k0[2][1]),(k0[3][0],g+2.5,k0[3][1]),(k0[1][0],g+2.45,k0[1][1]))
bs=c+u*(L2/2+0.8); box(MV2,MT2,MUV2,(bs[0],g-0.1,bs[1]),(0.07,2.8,0.07)); panel('sign_bus',bs,bs+u*0.01,-nrm*0+np.array([nrm[0],nrm[1]])*-1,g+2.0,g+2.6,0.6,0.5,0.06)
box(MV2,MT2,MUV2,(c[0]+nrm[0]*1.1,g+0.4,c[1]+nrm[1]*1.1),(2.6*abs(u[0])+0.4*abs(u[1]),0.08,2.6*abs(u[1])+0.4*abs(u[0])))  # bench
print('bus shelter at',c.round(1))
# --- parking lamp posts every ~25 m inside OSM parking lots (in main terrain)
LV,LT,LUV=G('metal'); LHV,LHT,LHUV=G('lamp_head')
for e in WY.values():
    if e.get('tags',{}).get('amenity')=='parking' and e['nodes'][0] in ND:
        r=ring(e['id']); a_=np.array(r)
        if a_[:,0].max()>531 or a_[:,0].min()<380 or a_[:,1].min()<-120: continue
        for x in np.arange(a_[:,0].min()+8,a_[:,0].max(),25):
            for z in np.arange(a_[:,1].min()+8,a_[:,1].max(),25):
                if inpoly((x,z),r):
                    g=Hc(x,z); tube(LV,LT,LUV,[(x,g-0.2,z),(x,g+7.6,z),(x+0.9,g+7.9,z)],0.07,8)
                    box(LHV,LHT,LHUV,(x+1.1,g+7.7,z),(0.7,0.18,0.35))
# --- Barral school complex (OSM 1009134376 school, 1009134377 pitch, 62598026 gym (DSM 8.2 m), 62596516 public (DSM 3.3 m); IGN ortho 2006-2010)
def drape(poly,mat,off,uvf,step=1.0):
    V,T,UV=G(mat); a_=np.array(poly); idx={}
    xs=np.arange(a_[:,0].min()-step,a_[:,0].max()+step,step); zs=np.arange(a_[:,1].min()-step,a_[:,1].max()+step,step)
    def vid(i,j):
        if (i,j) not in idx:
            x,z=xs[i],zs[j]; idx[(i,j)]=len(V); V.append((float(x),Hc(x,z)+off,float(z))); UV.append(uvf(x,z))
        return idx[(i,j)]
    for i in range(len(xs)-1):
        for j in range(len(zs)-1):
            if inpoly(((xs[i]+xs[i+1])/2,(zs[j]+zs[j+1])/2),poly):
                a=vid(i,j);b=vid(i+1,j);c=vid(i+1,j+1);d=vid(i,j+1); T.extend([a,d,c,a,c,b])
sp_=ring(1009134376); drape(sp_,'school_yard',0.07,lambda x,z:(x/16.0,z/16.0))
HIDE.extend([v for q in sp_ for v in q]); HIDEL.append(len(sp_))
pp=ring(1009134377); pa_=np.array(pp); pc=pa_.mean(0)
ed=[(np.linalg.norm(pa_[(i+1)%len(pa_)]-pa_[i]),i) for i in range(len(pa_))]; L_,i_=max(ed); ax=(pa_[(i_+1)%len(pa_)]-pa_[i_])/L_; ay=np.array([-ax[1],ax[0]])
ext=[(float(np.dot(q-pc,ax)),float(np.dot(q-pc,ay))) for q in pa_]; lx=max(abs(e[0]) for e in ext); ly=max(abs(e[1]) for e in ext)
drape(pp,'pitch',0.11,lambda x,z:(0.5+float(np.dot(np.array([x,z])-pc,ax))/(2*lx),0.5+float(np.dot(np.array([x,z])-pc,ay))/(2*ly)),0.8)
HIDE.extend([v for q in pp for v in q]); HIDEL.append(len(pp))
GLV,GLT,GLUV=G('white_trim')
for sgn in (-1,1):   # handball goals 3x2 m at each end
    gc=pc+ax*sgn*(lx-0.6); g=Hc(*gc)
    p1=gc-ay*1.5; p2=gc+ay*1.5; bk=-ax*sgn*1.0
    tube(GLV,GLT,GLUV,[(p1[0],g,p1[1]),(p1[0],g+2,p1[1]),(p2[0],g+2,p2[1]),(p2[0],g,p2[1])],0.05,8)
    tube(GLV,GLT,GLUV,[(p1[0],g+2,p1[1]),(p1[0]+bk[0],g,p1[1]+bk[1]),(p2[0]+bk[0],g,p2[1]+bk[1]),(p2[0],g+2,p2[1])],0.03,6)
print('pitch',pc.round(1),'len',round(2*lx,1),'wid',round(2*ly,1))
gr=ring(62598026); g_,t_=extrude(gr,8.2,'gym_wall','roof_gym'); print('gym top',round(t_,1))
extrude(ring(62596516),3.3,'store_wall')
# white low wall (muret, 0.7 m) between school yard and court
MWV,MWT,MWUV=G('white_trim')
for z in np.arange(219.0,265.0,2.0): box(MWV,MWT,MWUV,(372.6,Hc(372.6,z+1)-0.3,z+1),(0.22,1.0,2.02),0,1.0)
# preau: open arcade built into the ground floor of 62598486, on its court-facing (east) side
pr=ring(62598486); a_,b_,n_,L_=facing(pr,(386,241)); gy=max(Hc(*a_),Hc(*b_))
panel('dark',a_,b_,n_,gy-0.3,gy+2.9,L_*0.7,0.5,0.03)
d_=(b_-a_)/L_
for t in np.linspace(0.15,0.85,6):
    q=a_+(b_-a_)*t+n_*0.25; box(MWV,MWT,MWUV,(q[0],gy-0.3,q[1]),(0.4,3.2,0.4),math.atan2(d_[1],d_[0]),1.0)
q=a_+(b_-a_)*0.15+n_*0.25; q2=a_+(b_-a_)*0.85+n_*0.25; tube(MWV,MWT,MWUV,[(q[0],gy+2.95,q[1]),(q2[0],gy+2.95,q2[1])],0.22,4)
print('preau face',np.round(a_,1),np.round(b_,1))
# sidewalk/pavement of av. des Trois Fontaines reaching the foot of both fin stairways (no grass there)
swp=[(455.0,3.0),(472.0,3.0),(472.0,26.0),(448.0,26.0),(448.0,19.0),(452.5,14.0)]
drape(swp,'asphalt',0.06,lambda x,z:(x/4.0,z/4.0),0.8)
HIDE.extend([v for q in swp for v in q]); HIDEL.append(len(swp))
for m,(V,T,UV) in M.items(): addmesh('Store_'+m,m,V,T,UV)
# --- parking polygons: hide FBX surfaces + exclude grass/trees (asphalt drape built in gen_east.py)
for e in WY.values():
    t=e.get('tags',{})
    if t.get('amenity')=='parking' and t.get('parking','surface')=='surface' and e['nodes'][0] in ND:
        r=ring(e['id']); c=np.mean(r,0)
        if 380<c[0]<964 and -200<c[1]<300: HIDE.extend([v for q in r for v in q]); HIDEL.append(len(r))
