# ---- procedural 2010 buildings from OSM footprints (ODbL) + IGN nDSM heights + IGN 2006-2010 ortho roof colours ----
import math, random
from matplotlib.path import Path as _Path
from PIL import Image as _Im
_Im.MAX_IMAGE_PIXELS=None
_MNH=np.asarray(_Im.open('/workspace/v3/mnh.tif'),np.float32)
_ORT=np.asarray(_Im.open('/workspace/v3/full_ORTHOIMAGERY.ORTHOPHOTOS2006-2010.jpg').convert('RGB'),np.float32)
BM={}
def BG(m):
    if m not in BM: BM[m]=([],[],[])
    return BM[m]
def b_tri(m,p0,p1,p2,uv,want):
    V,T,UV=BG(m); p0,p1,p2=map(np.asarray,(p0,p1,p2)); n=np.cross(p1-p0,p2-p0)
    k=len(V); V.extend([tuple(p0),tuple(p1),tuple(p2)]); UV.extend(uv)
    T.extend([k,k+1,k+2] if np.dot(n,want)>=0 else [k,k+2,k+1])
def b_quad(m,p0,p1,p2,p3,uv,want):
    b_tri(m,p0,p1,p2,[uv[0],uv[1],uv[2]],want); b_tri(m,p0,p2,p3,[uv[0],uv[2],uv[3]],want)
def b_box(m,c,size,yaw,uvs=1.0):  # c bottom-centre (x,y,z); yaw rad along x
    cx,cy,cz=c; sx,sy,sz=size; u=np.array([math.cos(yaw),0,math.sin(yaw)]); v=np.array([-math.sin(yaw),0,math.cos(yaw)]); up=np.array([0,1.,0])
    o=np.array([cx,cy,cz])+up*sy/2
    for ax,e,a1,e1,a2,e2 in ((u,sx,v,sz,up,sy),(v,sz,u,sx,up,sy),(up,sy,u,sx,v,sz)):
        for sg in (-1,1):
            fc=o+ax*sg*e/2; p=[fc+a1*s1*e1/2+a2*s2*e2/2 for s1,s2 in ((-1,-1),(1,-1),(1,1),(-1,1))]
            b_quad(m,*p,[(0,0),(e1/uvs,0),(e1/uvs,e2/uvs),(0,e2/uvs)],ax*sg)
CELLC,CELLR=6,16
def cell(col,row,u0,u1,v0,v1):
    pu=1.5/1536; pv=1.5/4096
    U0=col/CELLC+pu; U1=(col+1)/CELLC-pu; V1=1-row/CELLR-pv; V0=1-(row+1)/CELLR+pv
    return [(U0+(U1-U0)*u0,V0+(V1-V0)*v0),(U0+(U1-U0)*u1,V0+(V1-V0)*v0),(U0+(U1-U0)*u1,V0+(V1-V0)*v1),(U0+(U1-U0)*u0,V0+(V1-V0)*v1)]
def obb(p):
    best=None
    for i in range(len(p)):
        d=p[(i+1)%len(p)]-p[i]; L=np.linalg.norm(d)
        if L<0.5: continue
        u=d/L; v=np.array([-u[1],u[0]]); a=p@u; b=p@v
        A=(a.max()-a.min())*(b.max()-b.min())
        if best is None or A<best[0]: best=(A,u,v,a.min(),a.max(),b.min(),b.max())
    A,u,v,a0,a1,b0,b1=best; c=u*(a0+a1)/2+v*(b0+b1)/2; ha=(a1-a0)/2; hb=(b1-b0)/2
    if hb>ha: u,v,ha,hb=v,-u,hb,ha
    return c,u,v,ha,hb,A
def earclip2(P):
    p=list(range(len(P))); T=[]; g=0
    while len(p)>3 and g<4000:
        g+=1; n=len(p); ok=False
        for i in range(n):
            a,b,c=P[p[i-1]],P[p[i]],P[p[(i+1)%n]]
            if (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])<=1e-9: continue
            tp=_Path([a,b,c])
            if any(tp.contains_point(P[k]) for k in p if k not in(p[i-1],p[i],p[(i+1)%n])): continue
            T+=[p[i-1],p[i],p[(i+1)%n]]; p.pop(i); ok=True; break
        if not ok: break
    if len(p)==3: T+=p
    return T
UP=np.array([0,1.,0]); DN=-UP
OFFICE=[False]
ENT=[]
def wall_bays(p,g,ytop,kind,tone,rnd,shutters=True):
    ENT.clear()
    n=len(p); storeys=1 if kind=='bakery' else max(1,int(round((ytop-g-0.3)/2.8))); sh_=(ytop-g)/storeys
    lens=[np.linalg.norm(p[(i+1)%n]-p[i]) for i in range(n)]; longest=int(np.argmax(lens))
    for i in range(n):
        a=p[i]; b=p[(i+1)%n]; L=lens[i]
        if L<0.3: continue
        d=(b-a)/L; out=np.array([d[1],0,-d[0]])
        nb=max(1,int(round(L/3.0))); bw=L/nb
        mid=nb//2; balc=kind=='apt' and L>9 and not OFFICE[0]
        for s in range(storeys):
            y0=g+s*sh_; y1=y0+sh_
            for j in range(nb):
                q0=a+d*bw*j; q1=a+d*bw*(j+1)
                if kind=='house':
                    if s==0: row=1 if (i==longest and j==mid) else (7 if L>2.5 else 2)
                    else: row=0 if L>2.5 else 2
                    if bw<2.0: row=2
                elif kind=='apt':
                    stair=(i==longest or (lens[i]>14 and nb>=5)) and j==mid
                    if s==0: row=6 if stair else 3
                    else: row=5 if stair else (4 if (balc and j%2==0) else 3)
                    if bw<2.0: row=2
                else: row=2
                if s==0 and row==3 and kind=='apt' and rnd.random()<0.3: row=2
                if row==2:
                    if kind=='house': row=rnd.choice([8,8,9,10,11,2])
                    elif kind=='apt': row=rnd.choice([8,9,9,11]) if s>0 else rnd.choice([9,11,2])
                    elif kind=='bakery': row=2
                    else: row=rnd.choice([2,11,11,10,9])
                if MZ[0] and kind=='apt' and bw>=2.0: row=(6 if (s==0 and i==longest and j==mid) else rnd.choice([12,13,14,3]))
                if kind=='apt' and row==3 and not OFFICE[0]: row=rnd.choice([3,3,12,13,14])      # roller shutters at varied heights
                if kind=='house' and row==0 and rnd.random()<0.4: row=15
                if s==0 and ((kind=='apt' and row==6) or (kind=='house' and row==1)): ENT.append((q0,q1,d,out,y0,bw))
                b_quad('facade',(q0[0],y0,q0[1]),(q1[0],y0,q1[1]),(q1[0],y1,q1[1]),(q0[0],y1,q0[1]),cell(tone,row,0.03,0.09,0.93,0.98) if kind=='bakery' else cell(tone,row,0,1,0.0,1.0),out)
                if kind=='apt' and row==4 and s>=1:
                    cm=(q0+q1)/2+np.array([out[0],out[2]])*0.65; yaw=math.atan2(d[1],d[0])
                    b_box('white_trim',(cm[0],y0-0.05,cm[1]),(bw*0.92,0.18,1.3),yaw)
                    fr=cm+np.array([out[0],out[2]])*0.6
                    b_box('white_trim',(fr[0],y0+0.13,fr[1]),(bw*0.92,0.95,0.1),yaw)
                    if rnd.random()<0.14:
                        dq=fr+d*bw*0.3+np.array([out[0],out[2]])*0.05; dish(dq,np.array([out[0],out[2]]),y0+1.35)
    if kind in('house','apt'):
        for ci in rnd.sample(range(n),min(2,n)):
            a=p[ci]; e1=p[ci]-p[ci-1]; e2=p[(ci+1)%n]-p[ci]
            n1=np.array([e1[1],-e1[0]])/max(1e-6,np.linalg.norm(e1)); n2=np.array([e2[1],-e2[0]])/max(1e-6,np.linalg.norm(e2))
            q=a+(n1+n2)*0.1-e2/max(1e-6,np.linalg.norm(e2))*0.25
            b_box('metal',(q[0],g,q[1]),(0.09,ytop-g,0.09),math.atan2(e2[1],e2[0]))
    return storeys
def dish(c,outv,y,r=0.32):
    # 2010 satellite dish: octagon disc tilted up, plus LNB arm
    o=np.array([outv[0],0.35,outv[1]]); o/=np.linalg.norm(o); s=np.array([-outv[1],0,outv[0]]); t=np.cross(o,s)
    C3=np.array([c[0],y,c[1]]); P=[C3+r*(math.cos(k*math.pi/4)*s+math.sin(k*math.pi/4)*t)-o*0.04*0 for k in range(8)]
    for k in range(8):
        b_tri('white_trim',C3-o*0.06,P[k],P[(k+1)%8],[(0,0)]*3,o); b_tri('white_trim',C3-o*0.06,P[k],P[(k+1)%8],[(0,0)]*3,-o)
    L=C3+o*0.35; b_box('metal',(L[0],L[1]-0.03,L[2]),(0.05,0.06,0.05),0)
    b_box('metal',(c[0],y-r-0.15,c[1]),(0.04,r+0.15,0.04),0)
def hip_roof(c,u,v,ha,hb,ye,pitch,mat,gable,tone,chim,rnd):
    o=0.45; A=ha+o; B=hb+o; rise=hb*math.tan(math.radians(pitch)); yr=ye+rise
    U3=np.array([u[0],0,u[1]]); V3=np.array([v[0],0,v[1]]); C3=np.array([c[0],0,c[1]])
    P=lambda s,t,y: C3+U3*s+V3*t+UP*y
    ye2=ye-o*math.tan(math.radians(pitch))
    ra=0 if gable else max(0.0,A-B)   # ridge half-length
    if gable: ra=A
    sc=lambda s,t:(s/2.5,t/2.5)
    sl=math.hypot(B,rise+ (ye-ye2))
    for sg in (-1,1):   # long slopes
        p0=P(-A,sg*B,ye2); p1=P(A,sg*B,ye2); p2=P(ra,0,yr); p3=P(-ra,0,yr)
        wn=V3*sg*rise+UP*B
        b_quad(mat,p0,p1,p2,p3,[sc(-A,0),sc(A,0),sc(ra,sl),sc(-ra,sl)],wn)
        b_quad('white_trim',p0,p1,p1+UP*0.0+ -UP*0.0+np.array([0,0.001,0]),p0+np.array([0,0.001,0]),[(0,0)]*4,-UP) if False else None
        # gutter
        gm=(p0+p1)/2+V3*sg*0.05; b_box('metal',(gm[0],ye2-0.16,gm[2]),(2*A,0.14,0.14),math.atan2(u[1],u[0]))
    for sg in (-1,1):   # ends
        if gable:
            gr=rnd.choice([8,9,11,2,8]); cq=cell(tone,gr,0,1,0.28,1); b_tri('facade',P(sg*ha,-hb,ye),P(sg*ha,hb,ye),P(sg*ha,0,ye+rise),[cq[0],cq[1],((cq[0][0]+cq[1][0])/2,cq[2][1])],U3*sg)
            # verge: thin end of slope
        else:
            p0=P(sg*A,-B,ye2); p1=P(sg*A,B,ye2); p2=P(sg*ra,0,yr)
            b_tri(mat,p0,p1,p2,[sc(-B,0),sc(B,0),sc(0,sl)],U3*sg*rise+UP*B)
    # soffit (underside of overhang)
    b_quad('white_trim',P(-A,-B,ye2-0.01),P(A,-B,ye2-0.01),P(A,B,ye2-0.01),P(-A,B,ye2-0.01),[(0,0)]*4,DN)
    if chim:
        s=rnd.uniform(-ha*0.5,ha*0.5); t=rnd.uniform(-hb*0.4,hb*0.4); cp=P(s,t,0)
        b_box('facade_chim',(cp[0],ye,cp[2]),(0.55,rise+0.9,0.55),math.atan2(u[1],u[0]))
        b_box('metal',(cp[0],ye+rise+0.9,cp[2]),(0.7,0.08,0.7),math.atan2(u[1],u[0]))
def flat_roof(p,ytop,mat,parapet,tone):
    tt=earclip2([tuple(q) for q in p])
    V,T,UV=BG(mat); k=len(V); yr=ytop
    for q in p: V.append((q[0],yr+0.02,q[1])); UV.append((q[0]/10,q[1]/10))
    for i in range(0,len(tt),3):
        a,b,c=[np.array([V[k+j][0],V[k+j][1],V[k+j][2]]) for j in (tt[i],tt[i+1],tt[i+2])]
        n=np.cross(b-a,c-a); T.extend([k+tt[i],k+tt[i+1],k+tt[i+2]] if n[1]>=0 else [k+tt[i],k+tt[i+2],k+tt[i+1]])
    if parapet>0:
        n=len(p)
        for i in range(n):
            a=p[i]; b=p[(i+1)%n]; L=np.linalg.norm(b-a)
            if L<0.2: continue
            d=(b-a)/L; out=np.array([d[1],0,-d[0]])
            b_quad('facade',(a[0],yr,a[1]),(b[0],yr,b[1]),(b[0],yr+parapet,b[1]),(a[0],yr+parapet,a[1]),cell(tone,2,0.03,0.09,0.93,0.98),out)
            b_quad('facade',(b[0],yr,b[1]),(a[0],yr,a[1]),(a[0],yr+parapet,a[1]),(b[0],yr+parapet,b[1]),cell(tone,2,0,1,0.1,0.4),-out)
            m=(a+b)/2-np.array([out[0],out[2]])*0.0; b_box('coping',(m[0],yr+parapet,m[1]),(L+0.35,0.09,0.42),math.atan2(d[1],d[0]))
def roof_colour(p):
    pa=_Path(p); xs=np.arange(p[:,0].min(),p[:,0].max(),1.0); zs=np.arange(p[:,1].min(),p[:,1].max(),1.0)
    cs=[]
    for x in xs:
        for z in zs:
            if pa.contains_point((x,z)):
                c=int((x+532)*2); r=int((354.646-z)*2)
                if 0<=r<_ORT.shape[0] and 0<=c<_ORT.shape[1]: cs.append(_ORT[r,c])
    if not cs: return 'roof_slate'
    m=np.median(np.array(cs),0); R,Gc,B=m
    if R-B>35 and R>105: return 'roof_terracotta'
    if R-B>12 and R<120: return 'roof_brown'
    return 'roof_slate'
def ndsm(p):
    pa=_Path(p); s=[]
    for x in np.arange(p[:,0].min()+0.5,p[:,0].max(),1.0):
        for z in np.arange(p[:,1].min()+0.5,p[:,1].max(),1.0):
            if pa.contains_point((x,z)):
                r=int(354.646-z); c=int(x+532)
                if 0<=r<_MNH.shape[0] and 0<=c<_MNH.shape[1]: s.append(_MNH[r,c])
    return np.array(s) if s else np.zeros(1)
_hp=[_Path(np.array(HIDE[2*o:2*(o+n)]).reshape(-1,2)) for o,n in zip(np.cumsum([0]+HIDEL[:-1]),HIDEL)]
_osm=json.load(open('/workspace/v3/big.json'))['elements']; _N={e['id']:e for e in _osm if e['type']=='node'}
import re as _re
ADDR=[]
for _e in _osm:
    _hn=_e.get('tags',{}).get('addr:housenumber')
    if not _hn or not _re.match(r'\d+',_hn): continue
    if _e['type']=='node': _ll=(_e['lon'],_e['lat'])
    elif _e['type']=='way' and all(n in _N for n in _e['nodes']): _ll=(np.mean([_N[n]['lon'] for n in _e['nodes']]),np.mean([_N[n]['lat'] for n in _e['nodes']]))
    else: continue
    ADDR.append(((_ll[0]-6.107796675787339)*77484.85,(_ll[1]-45.88915585177747)*111132.9,_re.match(r'\d+',_hn).group()))
_AXY=np.array([(a[0],a[1]) for a in ADDR]) if ADDR else np.zeros((0,2))
NPLATES=[0]
def porches(kind,c0):
    for (q0,q1,d,out,y0,bw) in list(ENT):
        m=(q0+q1)/2; o=np.array([out[0],out[2]]); yaw=math.atan2(d[1],d[0])
        q=m+o*0.55; b_box('coping',(q[0],y0+2.45,q[1]),((bw*0.8 if kind=='apt' else 1.6),0.12,1.1),yaw)      # entrance canopy
        q=m+o*0.45; b_box('coping',(q[0],y0-0.15,q[1]),((bw*0.7 if kind=='apt' else 1.4),0.3,0.9),yaw)      # entrance step
        if kind=='apt':
            for sg in (-1,1): q=m+d*sg*bw*0.38+o*1.0; b_box('metal',(q[0],y0,q[1]),(0.06,2.45,0.06),yaw)
        if not len(_AXY): continue
        dd=np.linalg.norm(_AXY-m,axis=1); k=int(np.argmin(dd))
        if dd[k]>(14 if kind=='apt' else 12): continue
        num=ADDR[k][2]; dw=0.13; x0=m+d*(0.75 if kind=='house' else bw*0.28)+o*0.04; yp=y0+2.05
        for i,ch in enumerate(num):                                                   # enamel blue house-number plate (digit atlas)
            a_=x0+d*dw*i; b_=a_+d*dw; u0=int(ch)/10.0; u1=u0+0.1
            b_quad('housenum',(a_[0],yp,a_[1]),(b_[0],yp,b_[1]),(b_[0],yp+0.2,b_[1]),(a_[0],yp+0.2,a_[1]),[(u0,0),(u1,0),(u1,1),(u0,1)],out)
        NPLATES[0]+=1
exec(open('/workspace/v3/names.py').read())
MAISON_TONE=2; MZ=[False]; BI={}; BL=[]; nb_=dict(house=0,apt=0,comm=0,small=0); rc_={}
for e in _osm:
    if e['type']!='way' or 'building' not in e.get('tags',{}) or e['nodes'][0]!=e['nodes'][-1]: continue
    if e['id'] in (62590936,): continue
    t=e['tags']
    p=[((_N[n]['lon']-6.107796675787339)*77484.85,(_N[n]['lat']-45.88915585177747)*111132.9) for n in e['nodes'][:-1] if n in _N]
    if len(p)<3: continue
    p=np.array(p); c0=p.mean(0)
    if not(-531<c0[0]<963 and -353<c0[1]<353): continue
    if any(h.contains_point(tuple(c0)) for h in _hp): continue
    ar=0.5*sum(p[i,0]*p[(i+1)%len(p),1]-p[(i+1)%len(p),0]*p[i,1] for i in range(len(p)))
    if ar<0: p=p[::-1]; ar=-ar
    if ar<8: continue
    rnd=random.Random(e['id']); s=ndsm(p); top=float(np.percentile(s,90)) if s.max()>2 else 0.0
    if s.size>=15 and s.max()<1.2: continue      # absent from the IGN surface model -> not built / not a closed building
    low=top<3.2 and s.size>=15
    if low and np.hypot(c0[0]-450.6,c0[1]-122)<20: continue   # low lean-to in front of Grand Frais hid the storefront
    if top<2.2: top=2.8*float(t.get('building:levels',2))+1.0 if not low else 2.8
    top=min(top,45.0)
    g=min(Hc(*q) for q in p)-0.25; yb=max(Hc(*q) for q in p)
    c,u,v,ha,hb,A=obb(p); rect=ar/A
    bt=t.get('building','yes')
    if bt in('apartments','residential') and top>7 or (top>9.5 and ar>180 and ar<4000): kind='apt'
    elif ar>700 or bt in('commercial','retail','industrial','warehouse','supermarket') or (low and ar>150): kind='comm'
    elif ar>260 and top>5: kind='apt'
    elif ar<45 or bt in('garage','garages','shed','roof','service'): kind='small'
    else: kind='house'
    MZ[0]=e['id']==62594137
    if e['id']==62594137: kind='apt'; top=max(top,10.6)   # rue de l'Orme (OSM 62594137 apartments, 3 levels; nDSM p50 10.6 m) -> flat roof, parapet, gravel
    OFFICE[0]=bool(np.hypot(c0[0]-258.4,c0[1]-244.3)<5)   # ASSEDIC Croix de Barral: rendered office, ribbon windows, nDSM 9.4 m -> 3 storeys
    if OFFICE[0]: kind='apt'
    MAISON=bool(np.hypot(c0[0]-336.2,c0[1]-191.4)<4)
    BAKERY=(e['id']==62585474)
    if BAKERY: kind='small'; top=min(top,4.8)   # Boulangerie Chevalier: standalone single-storey shop (nDSM 4.5-5.6 m)
    BI[e['id']]=(p,g,yb,kind,top); nb_[kind]+=1; BL.append((kind,float(c0[0]),float(c0[1]),float(top),float(ar),p.tolist()))
    if kind=='house':
        tone=rnd.randrange(5); pitch=rnd.uniform(28,38); rise=hb*math.tan(math.radians(pitch))
        ye=max(yb+2.6,g+0.25+top-rise); ye=min(ye,yb+9.5)
        wall_bays(p,g,ye,'house',tone,rnd); porches('house',c0)
        rm='roof_terracotta' if MAISON else roof_colour(p); rc_[rm]=rc_.get(rm,0)+1
        hip_roof(c,u,v,ha,hb,ye,pitch,rm,(not MAISON) and (rnd.random()<0.45 or rect<0.8),tone,True,rnd)
    elif kind=='apt':
        tone=5 if OFFICE[0] else (MAISON_TONE if MZ[0] else rnd.choice([5,5,0,1,2,3,4])); yt=max(yb+5.6,g+0.25+top-0.8)
        wall_bays(p,g,yt,'apt',tone,rnd)
        if not OFFICE[0]: porches('apt',c0)
        flat_roof(p,yt,rnd.choice(['roof_gravel','roof_gravel2','roof_gravel3']),0.8,tone)
        yaw=math.atan2(u[1],u[0]); U2=np.array(u); V2=np.array(v); _pp=_Path(p); IN=lambda q,r=1.8:_pp.contains_point(tuple(q),radius=-r) and _pp.contains_point(tuple(q),radius=r)
        nst=max(1,int(ha*2//22))
        for k in range(nst):   # lift / stair housing per stairwell
            s_=(-ha+ha*2*(k+0.5)/nst)*0.9; q=c+U2*s_
            if not IN(q,2.2): continue
            b_box('coping',(q[0],yt,q[1]),(3.2,2.4,2.6),yaw); b_box('metal',(q[0],yt+2.4,q[1]),(3.4,0.08,2.8),yaw)
        for k in range(rnd.randint(3,7)):  # ventilation stacks
            q=c+U2*rnd.uniform(-ha*0.85,ha*0.85)+V2*rnd.uniform(-hb*0.7,hb*0.7)
            if not IN(q): continue
            b_box('metal',(q[0],yt,q[1]),(0.45,0.7,0.45),yaw); b_box('metal',(q[0],yt+0.7,q[1]),(0.6,0.06,0.6),yaw)
        for k in range(rnd.randint(1,3)):  # TV antennas (rateau)
            q=c+U2*rnd.uniform(-ha*0.7,ha*0.7)+V2*rnd.uniform(-hb*0.5,hb*0.5); hh=rnd.uniform(2.5,4)
            if not IN(q): continue
            b_box('metal',(q[0],yt,q[1]),(0.05,hh,0.05),yaw)
            for j in range(3): b_box('metal',(q[0],yt+hh-0.3-j*0.45,q[1]),(1.4-j*0.3,0.03,0.03),yaw+rnd.uniform(-.3,.3))
        if rnd.random()<0.6:
            q=c+U2*rnd.uniform(-ha*0.6,ha*0.6)+V2*hb*0.4
            if IN(q): dish(q,V2,yt+1.0,0.45)
    elif kind=='comm':
        yt=max(yb+3.5,g+0.25+min(top,12)); n=len(p); lens=[np.linalg.norm(p[(i+1)%n]-p[i]) for i in range(n)]; li=int(np.argmax(lens))
        for i in range(n):
            a=p[i]; b=p[(i+1)%n]; L=lens[i]
            if L<0.2: continue
            d=(b-a)/L; out=np.array([d[1],0,-d[0]])
            b_quad(CLAD.get(e['id'],'cladding'),(a[0],g,a[1]),(b[0],g,b[1]),(b[0],yt,b[1]),(a[0],yt,a[1]),[(0,g/3),(L/3,g/3),(L/3,yt/3),(0,yt/3)],out)
            if i==li and L>8:
                m=(a+b)/2+np.array([out[0],out[2]])*0.06; w=min(10,L*0.4); q0=m-d*w/2; q1=m+d*w/2; yg=Hc(*m)
                b_quad('window',(q0[0],yg,q0[1]),(q1[0],yg,q1[1]),(q1[0],yg+3.0,q1[1]),(q0[0],yg+3.0,q0[1]),[(0,0),(1,0),(1,1),(0,1)],out)
                cm=m+np.array([out[0],out[2]])*1.4; b_box('metal',(cm[0],yg+3.2,cm[1]),(w+2,0.25,2.8),math.atan2(d[1],d[0]))
                for t_ in np.linspace(-w/2,w/2,int(w/1.5)+1):
                    q=m+d*t_+np.array([out[0],out[2]])*0.04; b_box('metal',(q[0],yg,q[1]),(0.08,3.0,0.1),math.atan2(d[1],d[0]))
                q=m+np.array([out[0],out[2]])*0.04; b_box('metal',(q[0],yg+2.2,q[1]),(w,0.07,0.1),math.atan2(d[1],d[0]))
                for sg_ in (-1,1):
                    q=cm+d*sg_*(w/2)+np.array([out[0],out[2]])*1.1; b_box('metal',(q[0],yg,q[1]),(0.14,3.2,0.14),0)
            if L>6:
                for t_ in np.arange(4,L-2,14):
                    q=a+d*t_+np.array([out[0],out[2]])*0.08; b_box('metal',(q[0],g,q[1]),(0.12,yt-g,0.12),math.atan2(d[1],d[0]))
        flat_roof(p,yt,'roof_gravel',0.6,2)
    else:
        tone=0 if BAKERY else rnd.randrange(6); yt=(Hc(236.1,319.3)+4.0) if BAKERY else max(yb+2.2,g+0.25+min(top,3.2))
        wall_bays(p,g,yt,'bakery' if BAKERY else 'small',tone,rnd); flat_roof(p,yt,'roof_gravel',0.3 if BAKERY else 0.0,tone)
        if BAKERY: print('bakery walls g',round(g,2),'yb',round(yb,2),'yt',round(yt,2))
# --- 2010 discount-store shopfronts (Grand Frais, Lidl): glazed entrance, mullions, steel canopy on posts, fascia
for wid,sg,park,ht,asp in STORES:
    if sg not in('sign_grandfrais','sign_lidl'): continue
    r=ring(wid); a,b_,n2,L=facing(r,park); a=np.array(a); b_=np.array(b_); d=(b_-a)/L; out=np.array([n2[0],0,n2[1]]); m=a+(b_-a)*0.5
    w=min(L*0.6,24); yg=Hc(*m)-0.05; yaw=math.atan2(d[1],d[0]); o2=np.array(n2)
    q0=m-d*w/2+o2*0.12; q1=m+d*w/2+o2*0.12
    b_quad('shopglass',(q0[0],yg+0.3,q0[1]),(q1[0],yg+0.3,q1[1]),(q1[0],yg+3.6,q1[1]),(q0[0],yg+3.6,q0[1]),[(0,0),(1,0),(1,1),(0,1)],out)
    for t_ in np.linspace(-w/2,w/2,int(w/1.6)+1):
        q=m+d*t_+o2*0.16; b_box('metal',(q[0],yg+0.3,q[1]),(0.1,3.3,0.12),yaw)
    for yy in (yg+0.3,yg+2.5,yg+3.55):
        q=m+o2*0.16; b_box('metal',(q[0],yy,q[1]),(w,0.08,0.12),yaw)
    q=m+o2*0.1; b_box('coping',(q[0],yg,q[1]),(w+0.4,0.3,0.25),yaw)
    cd_=3.2; cm=m+o2*(cd_/2+0.1); b_box('canopy',(cm[0],yg+4.0,cm[1]),(w+3,0.35,cd_),yaw)
    b_box('metal',(cm[0],yg+3.95,cm[1]),(w+3,0.05,cd_),yaw)
    for t_ in np.linspace(-(w+2)/2,(w+2)/2,max(2,int(w/6)+1)):
        q=m+d*t_+o2*(cd_-0.2); b_box('metal',(q[0],Hc(*q)-0.1,q[1]),(0.16,4.1-(Hc(*q)-0.1-yg),0.16),yaw)
    # sign above the glass entrance (between canopy and roof line)
    asp=5.33 if sg=='sign_grandfrais' else 1.0; top_=yg+ht-0.4; sh_=min(2.4,top_-(yg+4.6)) if asp<2 else min(1.5,top_-(yg+4.6)); sw=sh_*asp
    q0=m-d*sw/2+o2*0.2; q1=m+d*sw/2+o2*0.2; ys=yg+4.6
    if sg!='sign_grandfrais': b_quad(sg,(q0[0],ys,q0[1]),(q1[0],ys,q1[1]),(q1[0],ys+sh_,q1[1]),(q0[0],ys+sh_,q0[1]),[(0,0),(1,0),(1,1),(0,1)],out)
    # canopy fascia sign (front face of the canopy, raised fascia 0.75 m)
    fm=m+o2*(cd_+0.12); b_box('canopy',(fm[0]-o2[0]*0.06,yg+3.95,fm[1]-o2[1]*0.06),(w+3,0.8,0.1),yaw)
    fh=0.62; fw=fh*asp
    for t_ in ([] if asp>2 else [-(w+3)/2+1.2,(w+3)/2-1.2]):
        c_=fm+d*t_+o2*0.02; q0=c_-d*fw/2; q1=c_+d*fw/2
        b_quad(sg,(q0[0],yg+4.04,q0[1]),(q1[0],yg+4.04,q1[1]),(q1[0],yg+4.04+fh,q1[1]),(q0[0],yg+4.04+fh,q0[1]),[(0,0),(1,0),(1,1),(0,1)],out)
    if asp<2:   # hanging 'ENTREE' sign under the canopy
        c_=m+o2*(cd_*0.6); q0=c_-d*0.9; q1=c_+d*0.9
        b_quad('sign_entree',(q0[0],yg+3.25,q0[1]),(q1[0],yg+3.25,q1[1]),(q1[0],yg+3.75,q1[1]),(q0[0],yg+3.75,q0[1]),[(0,0),(1,0),(1,1),(0,1)],out)
        b_quad('sign_entree',(q1[0],yg+3.25,q1[1]),(q0[0],yg+3.25,q0[1]),(q0[0],yg+3.75,q0[1]),(q1[0],yg+3.75,q1[1]),[(0,0),(1,0),(1,1),(0,1)],-out)
        # Lidl pylon at the lot entrance side
        pq=m+o2*(cd_+9)+d*(w/2+5); gp=Hc(*pq)
        b_box('metal',(pq[0],gp-0.1,pq[1]),(0.3,6.2,0.3),yaw)
        for sgn in (1,-1):
            q0=pq-d*1.1+o2*sgn*0.2; q1=pq+d*1.1+o2*sgn*0.2
            b_quad(sg,(q0[0],gp+4.0,q0[1]),(q1[0],gp+4.0,q1[1]),(q1[0],gp+6.2,q1[1]),(q0[0],gp+6.2,q0[1]),[(0,0),(1,0),(1,1),(0,1)] if sgn>0 else [(1,0),(0,0),(0,1),(1,1)],out*sgn)
        b_box('metal',(pq[0],gp+3.9,pq[1]),(2.4,0.1,0.5),yaw)
    if sg=='sign_lidl':
        for side,(aa,bb) in enumerate([(a,b_)]+[(np.array(r[i]),np.array(r[(i+1)%len(r)])) for i in range(len(r)) if np.linalg.norm(np.array(r[(i+1)%len(r)])-np.array(r[i]))>15 and np.dot(np.array([r[(i+1)%len(r)][1]-r[i][1],-(r[(i+1)%len(r)][0]-r[i][0])]),n2)<0.2 and np.dot(np.array([r[(i+1)%len(r)][1]-r[i][1],-(r[(i+1)%len(r)][0]-r[i][0])]),n2)>-0.2][:1]):
            LL=np.linalg.norm(bb-aa); dd=(bb-aa)/LL; oo=np.array([dd[1],-dd[0]]); mq=(aa+bb)/2; gq=Hc(*((aa+bb)/2)); sz=2.6
            if side==0: gq=yg
            _P=np.array(BI[62590936][0]) if 62590936 in BI else np.array(r); off_=max(0.0,float(np.max((_P-mq)@oo))); mq=mq+oo*off_
            ys=gq+ht-0.4-sz-1.4; bq=mq+oo*0.6; b_box('canopy',(bq[0],ys-0.1,bq[1]),(sz+0.2,sz+0.2,0.3),math.atan2(dd[1],dd[0]))
            q0=mq-dd*sz/2+oo*0.8; q1=mq+dd*sz/2+oo*0.8; b_quad('sign_lidl',(q0[0],ys,q0[1]),(q1[0],ys,q1[1]),(q1[0],ys+sz,q1[1]),(q0[0],ys+sz,q0[1]),[(0,0),(1,0),(1,1),(0,1)],np.array([oo[0],0,oo[1]]))
            print('lidl facade logo',side,mq.round(1),round(ys,1))
    print('shopfront',sg,round(w,1),'mid',m.round(1),'n',o2.round(2),'yg',round(yg,1))
# ---- 2010 landmarks: Boulangerie Chevalier (OSM node 5987065910, Rue du Val Vert) and ASSEDIC office (OSM 'Pôle Emploi' node 9857997118, 6 rue du Champ de la Taillée)
_roads=[]
for e in _osm:
    if e['type']=='way' and e.get('tags',{}).get('highway') in('residential','tertiary','secondary','unclassified','primary','service','living_street'):
        _roads+= [((_N[n]['lon']-6.107796675787339)*77484.85,(_N[n]['lat']-45.88915585177747)*111132.9) for n in e['nodes'] if n in _N]
_roads=np.array(_roads); LM={}; PAVE=[[(428.9,104.5),(469.2,104.5),(469.2,124.4),(428.9,124.4)]]   # Grand Frais apron: lot reaches the storefront
for key,pt,sign,asp,vit in (('boulangerie',(233.5,317.9),'sign_boulangerie',5.12,'vitrine_boulangerie'),('assedic',(258.3,243.8),'sign_assedic',4.0,'shopglass'),('maison',(336.2,191.4),None,1,None)):
    pt=np.array(pt); best=None
    for e in _osm:
        if e['type']!='way' or 'building' not in e.get('tags',{}) or e['nodes'][0]!=e['nodes'][-1]: continue
        p=np.array([((_N[n]['lon']-6.107796675787339)*77484.85,(_N[n]['lat']-45.88915585177747)*111132.9) for n in e['nodes'][:-1] if n in _N])
        if len(p)<3: continue
        dd=0 if _Path(p).contains_point(tuple(pt)) else np.min(np.linalg.norm(p-pt,axis=1))
        if best is None or dd<best[0]: best=(dd,p,e['id'])
    dd,p,bid=best
    ar=0.5*sum(p[i,0]*p[(i+1)%len(p),1]-p[(i+1)%len(p),0]*p[i,1] for i in range(len(p)))
    if ar<0: p=p[::-1]
    rd=_roads[np.argmin(np.linalg.norm(_roads-p.mean(0),axis=1))]
    bestE=None
    for i in range(len(p)):
        a=p[i]; b_=p[(i+1)%len(p)]; L=np.linalg.norm(b_-a)
        if L<4: continue
        dv=(b_-a)/L; o2=np.array([dv[1],-dv[0]]); mm=(a+b_)/2
        sc=L*max(0,np.dot(o2,(rd-mm)/max(1e-3,np.linalg.norm(rd-mm))))+L*0.2
        if key=='boulangerie': sc=-abs(np.dot(pt-a,o2))-(0 if 0<=np.dot(pt-a,dv)<=L else 99)   # shopfront = wall carrying OSM node 5987065910
        if bestE is None or sc>bestE[0]: bestE=(sc,a,b_,dv,o2,L,mm)
    _,a,b_,dv,o2,L,mm=bestE; out=np.array([o2[0],0,o2[1]]); yaw=math.atan2(dv[1],dv[0]); yg=Hc(*mm)-0.05
    dep=float(np.clip(np.dot(rd-mm,o2)-1.0,{'assedic':10.0,'maison':6.0,'boulangerie':7.0}.get(key,3.0),14.0)); wf=(L+16.0) if key=='boulangerie' else min(L,14.0)+4
    PAVE.append([tuple(mm+dv*s1*wf/2+o2*t1) for s1,t1 in ((-1,-0.5),(1,-0.5),(1,dep),(-1,dep))])
    kq=mm+o2*dep; b_box('coping',(kq[0],Hc(*kq)-0.1,kq[1]),(wf,0.27,0.22),yaw)
    if key=='boulangerie':   # asphalt/sidewalk on the shop's right-hand (north-east) side out to the street
        PAVE.append([tuple(mm+dv*t1+o2*s1) for t1,s1 in ((-4,0.0),(26,0.0),(26,14.0),(-4,14.0))])
        PAVE.append([tuple(mm+dv*t1+o2*s1) for t1,s1 in ((9.2,-22.0),(30,-22.0),(30,0.5),(9.2,0.5))])   # behind the NE corner (right of the shot)
    if key=='assedic':   # paved link from the forecourt edge to the nearest OSM road/footway in front of the office
        _hw=np.array([((_N[n]['lon']-6.107796675787339)*77484.85,(_N[n]['lat']-45.88915585177747)*111132.9) for e in _osm if e['type']=='way' and 'highway' in e.get('tags',{}) for n in e['nodes'] if n in _N])
        _ok=_hw[(np.dot(_hw-kq,o2)>0.5)]; tgt=_ok[np.argmin(np.linalg.norm(_ok-kq,axis=1))]; lv=tgt-kq; ll=np.linalg.norm(lv); lu=lv/ll; ln=np.array([-lu[1],lu[0]])
        PAVE.append([tuple(kq+ln*2.0-lu*0.5),tuple(tgt+ln*2.0+lu*1.0),tuple(tgt-ln*2.0+lu*1.0),tuple(kq-ln*2.0-lu*0.5)])
        print('assedic link',kq.round(1),'->',tgt.round(1),round(ll,1))
    if key=='maison':
        cam=mm+o2*16; LM[key]=dict(bid=bid,mid=mm.round(1).tolist(),n=o2.round(2).tolist(),cam=cam.round(1).tolist()); print('landmark',key,LM[key]); continue
    w=min(L*0.7,9.0)
    q0=mm-dv*w/2+o2*0.12; q1=mm+dv*w/2+o2*0.12
    VT=2.25 if key=='boulangerie' else 2.7
    b_quad(vit,(q0[0],yg+0.4,q0[1]),(q1[0],yg+0.4,q1[1]),(q1[0],yg+VT,q1[1]),(q0[0],yg+VT,q0[1]),[(0,0),(1,0),(1,1),(0,1)],out)
    q=mm+o2*0.1; b_box('coping',(q[0],yg,q[1]),(w+0.3,0.4,0.22),yaw)
    for t_ in np.linspace(-w/2,w/2,int(w/1.8)+1):
        q=mm+dv*t_+o2*0.15; b_box('metal' if key=='assedic' else 'bakery_wood',(q[0],yg+0.4,q[1]),(0.1,VT-0.4,0.1),yaw)
    sh_=1.0 if key=='boulangerie' else 1.1; sw=min(sh_*asp,L*0.9); sh_=sw/asp
    FO=0.25 if key=='boulangerie' else 0.3; q0=mm-dv*sw/2+o2*FO; q1=mm+dv*sw/2+o2*FO
    SY=yg+2.75 if key=='boulangerie' else yg+2.95
    b_box('bakery_wood' if key=='boulangerie' else 'coping',((mm+o2*(FO-0.12))[0],SY-0.1,(mm+o2*(FO-0.12))[1]),(sw+0.2,sh_+0.2,0.1),yaw)
    b_quad(sign,(q0[0],SY,q0[1]),(q1[0],SY,q1[1]),(q1[0],SY+sh_,q1[1]),(q0[0],SY+sh_,q0[1]),[(0,0),(1,0),(1,1),(0,1)],out)
    if key=='boulangerie':   # striped awning over the vitrine
        am=mm+o2*0.75; b_box('awning',(am[0],yg+2.45,am[1]),(w,0.08,1.4),yaw)
        f=mm+o2*1.45; b_box('awning',(f[0],yg+2.2,f[1]),(w,0.3,0.05),yaw)
    cam=mm+o2*16
    LM[key]=dict(bid=bid,mid=mm.round(1).tolist(),n=o2.round(2).tolist(),cam=cam.round(1).tolist(),road=rd.round(1).tolist())
    print('landmark',key,LM[key])
# ---- Grand Frais car park structure (OSM 62591761, building=yes wall=no): open lower level at lot level + asphalt roof deck reached from the west (top of the Val Vert slope)
X0,X1,Z0,Z1=428.9,468.9,108.6,124.5; YL=5.05; YD=8.36; TH=0.35; CX=(X0+X1)/2; CZ=(Z0+Z1)/2
HX0,HX1,HZ0,HZ1=463.2,466.6,Z1-2.35,Z1-0.85                                   # slab opening over the internal stair
for (x0_,x1_,z0_,z1_) in ((X0,HX0,Z0,Z1),(HX1,X1,Z0,Z1),(HX0,HX1,Z0,HZ0),(HX0,HX1,HZ1,Z1)):
    b_box('concrete_wall',((x0_+x1_)/2,YD-TH,(z0_+z1_)/2),(x1_-x0_,TH,z1_-z0_),0.0)                 # deck slab
    ax0,ax1,az0,az1=max(x0_,X0+0.2),min(x1_,X1-0.2),max(z0_,Z0+0.2),min(z1_,Z1-0.2)
    if ax1>ax0 and az1>az0: b_box('asphalt',((ax0+ax1)/2,YD-0.02,(az0+az1)/2),(ax1-ax0,0.04,az1-az0),0.0)  # deck asphalt
for xx in np.arange(X0+0.25,X1,7.9):                                          # columns + beams of the open level
    for zz in (Z0+0.3,CZ,Z1-0.6): b_box('concrete_wall',(xx,YL-0.2,zz),(0.4,YD-TH-YL+0.2,0.4),0.0)
    b_box('concrete_wall',(xx,YD-TH-0.45,CZ),(0.4,0.45,Z1-Z0),0.0)
for zz in (Z0+0.3,Z1-0.6): b_box('concrete_wall',(CX,YD-TH-0.45,zz),(X1-X0,0.45,0.4),0.0)
for xx in np.arange(X0+3.0,X1-1.0,2.5):                                       # white bay lines (2 rows of 5 m + 6 m aisle)
    for zc in (Z0+2.7,Z1-2.7):
        if zc>CZ and HX0-0.3<xx<HX1+0.3: continue
        b_box('white_trim',(xx,YD+0.005,zc),(0.12,0.02,5.0),0.0)
for zc in (Z0+5.25,Z1-5.25): b_box('white_trim',(CX+1.3,YD+0.005,zc),(X1-X0-4.0,0.02,0.12),0.0)
# kerb + low parapet with railing on N, E, S (west side open = drive-in from the slope)
for (cx_,cz_,sx_,sz_) in ((CX,Z0+0.12,X1-X0,0.24),(CX,Z1-0.12,X1-X0,0.24),(X1-0.12,CZ,0.24,Z1-Z0)):
    b_box('coping',(cx_,YD,cz_),(sx_,0.15,sz_),0.0)
    b_box('concrete_wall',(cx_,YD,cz_),(sx_,0.9,sz_) if sx_>sz_ else (sx_,0.9,sz_),0.0)
    b_box('metal',(cx_,YD+1.05,cz_),(sx_,0.05,0.05) if sx_>sz_ else (0.05,0.05,sz_),0.0)
    for k in np.arange(-0.5,0.5001,1.6/max(sx_,sz_)):
        q=(cx_+k*sx_*(sx_>sz_),cz_+k*sz_*(sz_>sx_)); b_box('metal',(q[0],YD+0.9,q[1]),(0.05,0.17,0.05),0.0)
# internal concrete stair under the deck, against the Grand Frais store wall (north side), rising east into the slab opening
nst=int(round((YD-YL)/0.17)); ri=(YD-YL)/nst; tr=0.27; SXb=HX1-nst*tr; SZc=(HZ0+HZ1)/2
for k in range(nst):
    xk=SXb+k*tr; top=YL+(k+1)*ri; b_box('concrete_wall',(xk+tr/2,YL-0.2,SZc),(tr,top-YL+0.2,1.3),0.0)
    b_box('metal',(xk+tr/2,top+0.9,SZc-0.68),(tr+0.01,0.05,0.05),0.0)
    if k%3==0: b_box('metal',(xk+tr/2,top,SZc-0.68),(0.04,0.9,0.04),0.0)
b_box('concrete_wall',(SXb-0.6,YL-0.05,SZc),(1.2,0.12,1.4),0.0)                 # bottom landing
for (cx_,cz_,sx_,sz_) in (((HX0+HX1)/2,HZ0-0.05,HX1-HX0,0.06),((HX0+HX1)/2,HZ1+0.05,HX1-HX0,0.06),(HX0-0.05,SZc,0.06,HZ1-HZ0)):  # railing round the opening (east side open = top step)
    b_box('metal',(cx_,YD+1.0,cz_),(max(sx_,0.05),0.05,max(sz_,0.05)),0.0)
    for k in np.linspace(-0.5,0.5,4): b_box('metal',(cx_+k*sx_*(sx_>sz_),YD,cz_+k*sz_*(sz_>sx_)),(0.05,1.0,0.05),0.0)
PAVE.append([(398.0,108.6),(429.05,108.6),(429.05,124.5),(398.0,124.5)])   # asphalt drive-in from the Val Vert aisle onto the roof deck
print('GF carpark deck',YD,'lower',YL,'steps',nst,'internal stair x',round(SXb,1),'->',HX1,'z',round(SZc,2))
# ---- paving audit (Barral / Val Vert / Trois Fontaines): OSM surface parkings west of the east-tile drape + footways as 2.6 m strips
for e in _osm:
    t=e.get('tags',{})
    if e['type']!='way': continue
    r=[((_N[n]['lon']-6.107796675787339)*77484.85,(_N[n]['lat']-45.88915585177747)*111132.9) for n in e['nodes'] if n in _N]
    if len(r)<2: continue
    c=np.mean(r,0)
    if not(-150<c[0]<650 and -200<c[1]<400): continue
    if t.get('amenity')=='parking': pass
    elif t.get('highway') in('footway','pedestrian'):
        for i in range(len(r)-1):
            a=np.array(r[i]); bb=np.array(r[i+1]); L_=np.linalg.norm(bb-a)
            if L_<0.5: continue
            d_=(bb-a)/L_; n_=np.array([-d_[1],d_[0]])*1.3; a=a-d_*0.6; bb=bb+d_*0.6
            PAVE.append([tuple(a-n_),tuple(bb-n_),tuple(bb+n_),tuple(a+n_)])
UNPAVED=('unpaved','gravel','grass','dirt','ground','fine_gravel','grass_paver','compacted')
npk_=0
for e in _osm:
    t=e.get('tags',{})
    if e['type']!='way' or t.get('amenity')!='parking' or t.get('parking','surface')!='surface' or t.get('surface') in UNPAVED: continue
    r=[((_N[n]['lon']-6.107796675787339)*77484.85,(_N[n]['lat']-45.88915585177747)*111132.9) for n in e['nodes'] if n in _N]
    if len(r)<4: continue
    c=np.mean(r,0)
    if not(-531<c[0]<963 and -353<c[1]<353): continue
    if 380<c[0]<964 and e['id']!=574467194: continue   # east tile lots: already asphalt in the east road mesh
    PAVE.append(r[:-1] if r[0]==r[-1] else r); npk_+=1
print('paved car parks',npk_)
M.clear()
for pv in PAVE:
    drape(pv,'asphalt',0.15,lambda x,z:(x/4.0,z/4.0),0.7); HIDE.extend([v for q in pv for v in q]); HIDEL.append(len(pv))
for m,(V,T,UV) in M.items(): addmesh('Pave_'+m,m,V,T,UV)
print('paved polys',len(PAVE))
exec(open('/workspace/v3/street.py').read())
exec(open('/workspace/v3/signs_named.py').read())
for m,(V,T,UV) in BM.items():
    if m=='facade_chim': m2='facade'
    addmesh('Bld_'+m,'facade' if m=='facade_chim' else m,V,T,UV)
print('buildings',nb_,rc_)
json.dump(BL,open('/tmp/bld.json','w'))
