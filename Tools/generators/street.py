# ---- street furniture + markings from OSM (ODbL) over the whole map extent; all self-made geometry ----
from matplotlib.path import Path as _SP
from PIL import ImageFont as _IF
XL=(-531,963); ZL=(-353,353)
def W3(n): return ((_N[n]['lon']-6.107796675787339)*77484.85,(_N[n]['lat']-45.88915585177747)*111132.9)
def inx(q,m=0): return XL[0]+m<q[0]<XL[1]-m and ZL[0]+m<q[1]<ZL[1]-m
RW={'primary':9,'secondary':8,'tertiary':7,'residential':6,'unclassified':5.5,'service':4,'living_street':5,'primary_link':6,'trunk_link':6,'tertiary_link':5,'busway':6}
AUD={}
def aud(k,n=1): AUD[k]=AUD.get(k,0)+n
_ways=[e for e in _osm if e['type']=='way']
ROADS=[]; use={}
for e in _ways:
    hw=e.get('tags',{}).get('highway')
    if hw in RW and e['nodes'][0] in _N:
        pts=[np.array(W3(n)) for n in e['nodes'] if n in _N]
        if not any(inx(p) for p in pts): continue
        ROADS.append((e,hw,pts)); aud('road:'+hw)
        for n in e['nodes']: use[n]=use.get(n,0)+1
for e in _ways:   # footway/path junctions also break kerbs
    if e.get('tags',{}).get('highway') in('footway','path','cycleway','steps','pedestrian'):
        for n in e['nodes']:
            if n in use: use[n]+=1
JN=np.array([W3(n) for n,c in use.items() if c>1 and n in _N]) if use else np.zeros((0,2))
BPATH=[_SP(np.array(b[5])) for b in BL]; BC=np.array([(b[1],b[2]) for b in BL])
def inbld(q,r=0.6):
    d=np.hypot(BC[:,0]-q[0],BC[:,1]-q[1])
    return any(BPATH[i].contains_point(tuple(q),radius=r) or BPATH[i].contains_point(tuple(q),radius=-r) for i in np.where(d<60)[0])
def near_j(q,r): return len(JN) and np.min(np.hypot(JN[:,0]-q[0],JN[:,1]-q[1]))<r
def ry(x,z):   # road surface height: east tile roads are a mesh at H+0.06, west roads are terrain paint
    return Hc(x,z)+(0.075 if x>531 else 0.03)
def strip(m,a,b,w,y_off=0.0):
    d=b-a; L=np.linalg.norm(d)
    if L<0.05: return
    d/=L; n=np.array([-d[1],d[0]])*w/2; k=max(1,int(L/1.5))
    for i in range(k):
        p0=a+d*L*i/k; p1=a+d*L*(i+1)/k
        Q=[p0-n,p1-n,p1+n,p0+n]; b_quad(m,*[(q[0],ry(*q)+y_off,q[1]) for q in Q],[(0,0),(1,0),(1,1),(0,1)],UP)
def dens2(pts,s):
    out=[]
    for a,b in zip(pts[:-1],pts[1:]):
        n=max(1,int(np.linalg.norm(b-a)/s)); out+=[a+(b-a)*k/n for k in range(n)]
    out.append(pts[-1]); return np.array(out)
# --- road markings (2010 French: dashed axis T1 on 2-way roads >=5.5 m, continuous edge lines on primary/secondary/tertiary), kerbs
nm=nk=0; KB=[]
for e,hw,pts in ROADS:
    w=RW[hw]; t=e['tags']; oneway=t.get('oneway')=='yes'
    P=dens2(pts,0.5); s=0.0
    for i in range(len(P)-1):
        a,b=P[i],P[i+1]; s+=np.linalg.norm(b-a); m=(a+b)/2
        if not inx(m,2) or near_j(m,w*0.9): continue
        d=(b-a)/max(1e-6,np.linalg.norm(b-a)); n=np.array([-d[1],d[0]])
        if hw not in('service','living_street') and w>=5.5 and not oneway and (s%13.0)<3.0: strip('paint',a,b,0.12); nm+=1
        if hw in('primary','secondary','tertiary','primary_link','busway') :
            for sg in (-1,1): strip('paint',a+n*sg*(w/2-0.35),b+n*sg*(w/2-0.35),0.12)
        if hw not in('service',) and t.get('tunnel')!='yes' and t.get('bridge')!='yes':
            for sg in (-1,1):
                q=m+n*sg*(w/2+0.08)
                if inbld(q,0.3): continue
                KB.append((q,d,np.linalg.norm(b-a),sg*n,sg))
# merge consecutive 0.5 m kerb pieces into runs (<=4 m), emitted as top + two faces (12 verts per run)
_run=None
def _flush(rn):
    global nk
    if rn is None: return
    p0,p1,o=rn; L_=np.linalg.norm(p1-p0)
    if L_<0.05: return
    o3=np.array([o[0],0,o[1]]); A=[p0-o*0.08,p1-o*0.08,p1+o*0.08,p0+o*0.08]; ya=[ry(*q)+0.15 for q in A]
    b_quad('kerb',*[(q[0],y,q[1]) for q,y in zip(A,ya)],[(0,0),(L_,0),(L_,.2),(0,.2)],UP)
    for sg_,(i0,i1) in ((-1,(0,1)),(1,(3,2))):
        q0,q1=A[i0],A[i1]; b_quad('kerb',(q0[0],ry(*q0)-0.12,q0[1]),(q1[0],ry(*q1)-0.12,q1[1]),(q1[0],ya[i1],q1[1]),(q0[0],ya[i0],q0[1]),[(0,0),(L_,0),(L_,.27),(0,.27)],o3*sg_)
    nk+=1
_runs={}
for q,d,L_,o,sd in KB:
    p0=q-d*L_/2; p1=q+d*L_/2; _run=_runs.get(sd)
    if _run is not None and np.linalg.norm(_run[1]-p0)<0.3 and np.dot(o,_run[2])>0.995 and np.linalg.norm(_run[1]-_run[0])<4.0: _runs[sd]=(_run[0],p1,_run[2])
    else: _flush(_run); _runs[sd]=(p0,p1,o)
for _r in _runs.values(): _flush(_r)
# --- OSM street trees: natural=tree nodes + natural=tree_row ways (one tree per ~8 m) -> terrain tree instances
OT=[]
for e in _osm:
    t=e.get('tags',{})
    if e['type']=='node' and t.get('natural')=='tree':
        q=np.array(W3(e['id'])) if e['id'] in _N else None
        if q is not None and inx(q): OT+= [float(q[0]),float(q[1]),float(t.get('height','9')) if str(t.get('height','x')).replace('.','',1).isdigit() else 9.0,0]
ntr=0
for e in _ways:
    t=e.get('tags',{})
    if t.get('natural')!='tree_row': continue
    pts=[np.array(W3(n)) for n in e['nodes'] if n in _N]
    for q in dens2(pts,8.0):
        if inx(q) and not inbld(q,1.0): OT+=[float(q[0]),float(q[1]),9.0,0]; ntr+=1
json.dump({'trees':OT,'shrubs':[]},open('/workspace/valvert/clean/osm_trees.json','w'))
aud('built:tree_row_trees',ntr); aud('built:tree_node_trees',len(OT)//4-ntr)
aud('built:axis_dash_segments',nm); aud('built:kerb_segments',nk)
def road_at(q):
    best=None
    for e,hw,pts in ROADS:
        for a,b in zip(pts[:-1],pts[1:]):
            d=b-a; L=np.linalg.norm(d)
            if L<0.1: continue
            t_=np.clip(np.dot(q-a,d)/L/L,0,1); dd=np.linalg.norm(a+d*t_-q)
            if best is None or dd<best[0]: best=(dd,d/L,RW[hw],a+d*t_,hw)
    return best
# --- nodes
NT=[(e,e.get('tags',{})) for e in _osm if e['type']=='node' and e.get('tags') and inx((( e['lon']-6.107796675787339)*77484.85,(e['lat']-45.88915585177747)*111132.9))]
def pole(q,h,mat='metal',r=0.05): g=Hc(*q); b_box(mat,(q[0],g-0.2,q[1]),(r*2,h+0.2,r*2),0)
def panel_q(mat,c,d,y0,w,h,o):   # vertical textured quad facing o (2d), centre c
    q0=c-d*w/2; q1=c+d*w/2; out=np.array([o[0],0,o[1]])
    b_quad(mat,(q0[0],y0,q0[1]),(q1[0],y0,q1[1]),(q1[0],y0+h,q1[1]),(q0[0],y0+h,q0[1]),[(0,0),(1,0),(1,1),(0,1)],out)
    b_quad('metal',(q1[0],y0,q1[1]),(q0[0],y0,q0[1]),(q0[0],y0+h,q0[1]),(q1[0],y0+h,q1[1]),[(0,0)]*4,-out)
LAMPS=[]
def lamp(q,face):
    g=Hc(*q); b_box('lamp_pole',(q[0],g-0.2,q[1]),(0.14,8.2,0.14),0)
    hq_=q+face*1.2; b_box('lamp_pole',((q[0]+hq_[0])/2,g+7.85,(q[1]+hq_[1])/2),(0.08,0.08,0.08),0)
    b_box('lamp_pole',(hq_[0],g+7.75,hq_[1]),(0.7,0.2,0.36),math.atan2(face[1],face[0])); b_box('lamp_lens',(hq_[0],g+7.72,hq_[1]),(0.55,0.04,0.28),math.atan2(face[1],face[0]))
    arm=[q+face*k*0.3 for k in range(5)]
    for a_ in arm: b_box('lamp_pole',(a_[0],g+7.95,a_[1]),(0.3,0.07,0.07),math.atan2(face[1],face[0]))
    LAMPS.append(q)
for e,t in NT:
    q=np.array(W3(e['id']))
    hw=t.get('highway'); am=t.get('amenity'); mm_=t.get('man_made'); br=t.get('barrier')
    if hw=='crossing' or t.get('crossing') in('marked','uncontrolled','traffic_signals','zebra'):
        aud('osm:crossing'); r=road_at(q)
        if r is None or r[0]>3 or t.get('crossing') in('unmarked','informal','no'): continue
        _,d,w,c,_h=r; n=np.array([-d[1],d[0]])
        for s_ in np.arange(-w/2+0.5,w/2-0.3,1.0): strip('paint',c+n*s_-d*1.25+n*0,c+n*s_+d*1.25,0.5,0.005)   # zebra bars (0.5 m, 0.5 m gap, 2.5 m long)
        aud('built:zebra'); 
        if t.get('crossing')=='traffic_signals': aud('osm:crossing_signals')
    elif hw=='traffic_signals':
        aud('osm:traffic_signals'); r=road_at(q)
        if r is None: continue
        _,d,w,c,_h=r; n=np.array([-d[1],d[0]])
        for sg in (-1,1):
            p=c+n*sg*(w/2+0.8); g=Hc(*p); pole(p,3.0,'lamp_pole',0.06)
            b_box('tl_head',(p[0],g+2.2,p[1]),(0.32,0.95,0.28),math.atan2(d[1],d[0]))
            for k,mt in enumerate(('tl_red','tl_amber','tl_green')): 
                f=p+d*sg*0.15; b_box(mt,(f[0],g+2.85-k*0.3,f[1]),(0.2,0.2,0.04),math.atan2(n[1],n[0]))
        aud('built:traffic_light_poles',2); print('TLPOS',np.round(p,1),np.round(d,2))
    elif hw in('stop','give_way'):
        aud('osm:'+hw); r=road_at(q)
        if r is None or r[0]>8: continue
        _,d,w,c,_h=r; n=np.array([-d[1],d[0]]); p=c+n*(w/2+0.7)
        if inbld(p,0.2): continue
        pole(p,2.3,'lamp_pole',0.035); panel_q('sign_'+hw,p+np.array([0,0]),n,Hc(*p)+1.6,0.7,0.7,d); aud('built:'+hw+'_sign')
        if hw=='stop': strip('paint',c-n*w/2*0+n*0.2-d*0.0,c+n*(w/2-0.3),0.5,0.004)
    elif hw=='street_lamp':
        aud('osm:street_lamp'); r=road_at(q); lamp(q,(-r[1][::-1]*np.array([-1,1])) if False else (np.array(r[3])-q)/max(1e-3,np.linalg.norm(np.array(r[3])-q))); aud('built:lamp_osm')
    elif hw=='bus_stop' or (am=='shelter' and t.get('shelter_type')=='public_transport'):
        aud('osm:'+('bus_stop' if hw else 'shelter')); r=road_at(q)
        if r is None: continue
        _,d,w,c,_h=r; to=(c-q); to=to/max(1e-3,np.linalg.norm(to)); g=Hc(*q)
        if hw=='bus_stop':
            pole(q,2.9,'lamp_pole',0.04); panel_q('sign_bus',q+to*0.05,np.array([-to[1],to[0]]),g+2.2,0.5,0.6,to); aud('built:bus_pole')
        else:   # 2010 Sibra shelter: steel frame, glass back + sides, flat roof, bench
            u=np.array([-to[1],to[0]]); bk=-to; L2,D2=3.6,1.4; yaw=math.atan2(u[1],u[0])
            cs=[q-u*L2/2,q+u*L2/2,q-u*L2/2+bk*D2,q+u*L2/2+bk*D2]
            for c_ in cs: b_box('metal',(c_[0],g-0.2,c_[1]),(0.08,2.6,0.08),0)
            for a_,b_ in ((cs[2],cs[3]),(cs[0],cs[2]),(cs[1],cs[3])):
                o3=np.cross([b_[0]-a_[0],0,b_[1]-a_[1]],[0,1,0]); b_quad('glass',(a_[0],g+0.15,a_[1]),(b_[0],g+0.15,b_[1]),(b_[0],g+2.25,b_[1]),(a_[0],g+2.25,a_[1]),[(0,0),(1,0),(1,1),(0,1)],o3)
            rc=q+bk*D2/2; b_box('roof_shelter',(rc[0],g+2.4,rc[1]),(L2+0.3,0.12,D2+0.4),yaw)
            bc=q+bk*(D2-0.4); b_box('metal',(bc[0],g+0.42,bc[1]),(L2*0.7,0.06,0.4),yaw)
            aud('built:shelter')
    elif am=='bench':
        aud('osm:bench'); r=road_at(q); d=r[1] if r else np.array([1.,0]); yaw=math.atan2(d[1],d[0]); g=Hc(*q)
        b_box('bench_wood',(q[0],g+0.42,q[1]),(1.8,0.06,0.45),yaw); n=np.array([-d[1],d[0]]); bq=q-n*0.2
        b_box('bench_wood',(bq[0],g+0.5,bq[1]),(1.8,0.4,0.05),yaw)
        for sg in (-1,1): lq=q+d*sg*0.75; b_box('metal',(lq[0],g-0.1,lq[1]),(0.06,0.55,0.45),yaw)
        aud('built:bench')
    elif am=='waste_basket':
        aud('osm:waste_basket'); g=Hc(*q); b_box('bin_green',(q[0],g+0.3,q[1]),(0.4,0.55,0.4),0); pole(q,0.9,'metal',0.03); aud('built:waste_basket')
    elif am=='recycling':
        aud('osm:recycling'); g=Hc(*q)
        for k in range(3): b_box('bin_green' if k!=1 else 'bin_grey',(q[0]+k*1.7-1.7,g-0.05,q[1]),(1.5,1.7,1.5),0)
        aud('built:recycling_bins',3)
    elif mm_=='street_cabinet':
        aud('osm:street_cabinet'); g=Hc(*q); r=road_at(q); d=r[1] if r else np.array([1.,0]); b_box('cabinet',(q[0],g-0.1,q[1]),(0.9,1.35,0.4),math.atan2(d[1],d[0])); aud('built:street_cabinet')
    elif t.get('power')=='pole' or mm_=='utility_pole':
        aud('osm:pole'); g=Hc(*q); b_box('pole_wood' if t.get('material','wood')=='wood' else 'concrete_wall',(q[0],g-0.3,q[1]),(0.24,9.3,0.24),0); aud('built:pole')
    elif br in('bollard','block','kerb','lift_gate','gate','swing_gate','cycle_barrier'):
        aud('osm:barrier_'+br); g=Hc(*q)
        if br in('bollard','block'): b_box('bollard_m',(q[0],g-0.1,q[1]),(0.18,1.0,0.18),0); aud('built:bollard')
        elif br in('lift_gate',): 
            r=road_at(q); d=r[1] if r else np.array([1.,0]); n=np.array([-d[1],d[0]]); w=r[2] if r else 4
            pole(q+n*w/2,1.0,'metal',0.12); m_=q; b_box('barrier_rw',(m_[0],g+0.95,m_[1]),(0.08,0.08,w),math.atan2(d[1],d[0])); aud('built:lift_gate')
        elif br=='cycle_barrier':
            r=road_at(q); d=r[1] if r else np.array([1.,0])
            for sg in(-1,1): p=q+d*sg*0.9; b_box('metal',(p[0],g,p[1]),(0.05,1.0,1.4),math.atan2(d[1],d[0])); 
            aud('built:cycle_barrier')
    elif am in('post_box','letter_box'):
        aud('osm:post_box'); g=Hc(*q); b_box('postbox',(q[0],g+0.6,q[1]),(0.45,0.6,0.35),0); pole(q,0.6,'metal',0.03); aud('built:post_box')
    elif am=='bicycle_parking':
        aud('osm:bicycle_parking'); g=Hc(*q)
        for k in range(4):
            p=q+np.array([k*0.8-1.2,0]); b_box('metal',(p[0],g,p[1]),(0.05,0.8,0.7),0)
        aud('built:bike_rack')
    elif t.get('traffic_sign'):
        aud('osm:traffic_sign'); r=road_at(q)
        if r is None: continue
        _,d,w,c,_h=r; n=np.array([-d[1],d[0]]); p=c+n*(w/2+0.7) if r[0]<w/2+0.3 else q
        pole(p,2.3,'lamp_pole',0.035); panel_q('sign_limit',p,n,Hc(*p)+1.6,0.6,0.6,d); aud('built:traffic_sign')
# --- street lamps along public roads: OSM maps only 15 lamp nodes; IGN 2006-2010 ortho shows lamp shadows ~every 25-30 m along Barral / Val Vert streets -> placed every 28 m, staggered (inferred spacing)
for e,hw,pts in ROADS:
    if hw in('service','living_street') : continue
    w=RW[hw]; P=dens2(pts,1.0); s=0.0; side=1
    for i in range(1,len(P)):
        s+=np.linalg.norm(P[i]-P[i-1])
        if s<28: continue
        s=0; d=P[i]-P[i-1]; d/=max(1e-6,np.linalg.norm(d)); n=np.array([-d[1],d[0]])*side; side=-side
        q=P[i]+n*(w/2+0.9)
        if not inx(q,3) or near_j(q,4) or inbld(q,0.8): continue
        if LAMPS and np.min(np.hypot(*(np.array(LAMPS)-q).T))<14: continue
        lamp(q,-n); aud('built:lamp_inferred')
# --- linear barriers: fences, walls, hedges, retaining walls, cliffs, tree rows
for e in _ways:
    t=e.get('tags',{}); br=t.get('barrier'); nat=t.get('natural')
    if br not in('fence','wall','hedge','retaining_wall','guard_rail') and nat!='cliff': continue
    pts=[np.array(W3(n)) for n in e['nodes'] if n in _N]
    if not any(inx(p) for p in pts): continue
    aud('osm:'+(br or 'cliff')); P=dens2(pts,2.0)
    for a,b in zip(P[:-1],P[1:]):
        if not inx(a): continue
        d=b-a; L=np.linalg.norm(d); yaw=math.atan2(d[1],d[0]); m=(a+b)/2; g=min(Hc(*a),Hc(*b))
        if br=='hedge': b_box('hedge',(m[0],g-0.1,m[1]),(L+0.3,1.5,0.9),yaw)
        elif br=='fence':
            b_quad('fence_green',(a[0],Hc(*a),a[1]),(b[0],Hc(*b),b[1]),(b[0],Hc(*b)+1.6,b[1]),(a[0],Hc(*a)+1.6,a[1]),[(0,0),(L/2.5,0),(L/2.5,1),(0,1)],np.array([d[1],0,-d[0]]))
            b_quad('fence_green',(b[0],Hc(*b),b[1]),(a[0],Hc(*a),a[1]),(a[0],Hc(*a)+1.6,a[1]),(b[0],Hc(*b)+1.6,b[1]),[(0,0),(L/2.5,0),(L/2.5,1),(0,1)],-np.array([d[1],0,-d[0]]))
            b_box('green_metal',(a[0],Hc(*a)-0.2,a[1]),(0.06,1.8,0.06),0)
        elif br in('wall','retaining_wall') or nat=='cliff':
            wh_=float(t.get('height',1.2 if br=='wall' else 2.0)) if str(t.get('height','x')).replace('.','',1).isdigit() else (1.2 if br=='wall' else 2.0)
            b_box('concrete_wall',(m[0],g-0.3,m[1]),(L+0.05,wh_+0.3,0.25),yaw)
        else: b_box('metal',(m[0],g+0.5,m[1]),(L,0.3,0.05),yaw)
    aud('built:'+(br or 'cliff'))
for e in _ways:
    t=e.get('tags',{})
    if t.get('natural')=='tree_row' or t.get('highway') in('footway','path','cycleway','steps','pedestrian','platform') or t.get('landuse') or t.get('leisure') or t.get('building'):
        pts=[W3(n) for n in e['nodes'] if n in _N]
        if pts and any(inx(p) for p in pts):
            k_=('osm:tree_row' if t.get('natural') else 'osm:'+('highway_'+t['highway'] if t.get('highway') else 'landuse_'+t['landuse'] if t.get('landuse') else 'leisure_'+t['leisure'] if t.get('leisure') else 'building')); aud(k_)
for e,t in NT:
    if t.get('natural')=='tree': aud('osm:tree_node')
# --- car-park bay markings in OSM surface car parks (2.5 m bays, 5 m deep, along every edge > 10 m)
nb2=0
for e in _ways:
    t=e.get('tags',{})
    if t.get('amenity')!='parking' or t.get('parking','surface')!='surface' or e['nodes'][0]!=e['nodes'][-1] or t.get('surface') in UNPAVED: continue
    r=np.array([W3(n) for n in e['nodes'][:-1] if n in _N])
    if len(r)<3 or not inx(r.mean(0)): continue
    aud('osm:parking_surface'); pa_=_SP(r); ar=0.5*sum(r[i,0]*r[(i+1)%len(r),1]-r[(i+1)%len(r),0]*r[i,1] for i in range(len(r)))
    if ar<0: r=r[::-1]
    if abs(ar)<120 or e['id'] in(574467194,): continue   # tiny lots / GF lot already striped
    for i in range(len(r)):
        a=r[i]; b=r[(i+1)%len(r)]; L=np.linalg.norm(b-a)
        if L<10: continue
        d=(b-a)/L; n=np.array([-d[1],d[0]])     # inward for CCW
        for s_ in np.arange(1.0,L-0.9,2.5):
            p0=a+d*s_+n*0.4; p1=p0+n*5.0
            if not(pa_.contains_point(tuple(p1),radius=-0.3) or pa_.contains_point(tuple(p1),radius=0.3)) or inbld(p1,0.3): continue
            m=(p0+p1)/2; y=Hc(*m)+0.17; b_quad('paint',(p0[0]-d[0]*0.06,y,p0[1]-d[1]*0.06),(p0[0]+d[0]*0.06,y,p0[1]+d[1]*0.06),(p1[0]+d[0]*0.06,Hc(*p1)+0.17,p1[1]+d[1]*0.06),(p1[0]-d[0]*0.06,Hc(*p1)+0.17,p1[1]-d[1]*0.06),[(0,0)]*4,UP); nb2+=1
aud('built:parking_bay_lines',nb2)
print('STREET AUDIT',json.dumps(AUD,sort_keys=True))
json.dump(AUD,open('/workspace/v3/gen/audit.json','w'),indent=1,sort_keys=True)
